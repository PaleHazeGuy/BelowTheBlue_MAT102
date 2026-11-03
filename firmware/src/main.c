#include <stdio.h>
#include <stdint.h>
#include <stdbool.h>
#include <math.h>

#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include "driver/gpio.h"
#include "esp_adc/adc_oneshot.h"
#include "esp_timer.h"
#include "esp_bt.h"
#include "esp_bt_main.h"
#include "esp_gap_bt_api.h"
#include "esp_spp_api.h"
#include "nvs_flash.h"

#define MOCK_INPUT 0

#define PIN_LED_GREEN GPIO_NUM_25
#define PIN_LED_RED GPIO_NUM_26
#define PIN_BUTTON GPIO_NUM_27
#define CH_STICK_X ADC_CHANNEL_6
#define CH_STICK_Y ADC_CHANNEL_7

#define INVERT_X 0
#define INVERT_Y 0

#define SEND_PERIOD_MS 20
#define DEADZONE 0.10f
#define INTERACT_HOLD_MS 120
#define DEBOUNCE_MS 15
#define STALL_FAULT_MS 1500
#define BLINK_WAITING_MS 500
#define BLINK_FAULT_MS 125

static uint32_t client = 0;
static volatile bool ready = false;
static volatile bool bt_fault = false;

static adc_oneshot_unit_handle_t adc;
static int center_x = 2048;
static int center_y = 2048;

static int64_t now_ms(void) {
    return esp_timer_get_time() / 1000;
}

static void bt_cb(esp_spp_cb_event_t event, esp_spp_cb_param_t *p) {
    switch (event) {
        case ESP_SPP_INIT_EVT:
            ESP_ERROR_CHECK(esp_bt_gap_set_device_name("Sigma Controller"));
            ESP_ERROR_CHECK(esp_spp_start_srv(
                ESP_SPP_SEC_NONE, ESP_SPP_ROLE_SLAVE, 0, "Serial"));
            break;

        case ESP_SPP_START_EVT:
            ESP_ERROR_CHECK(esp_bt_gap_set_scan_mode(
                ESP_BT_CONNECTABLE, ESP_BT_GENERAL_DISCOVERABLE));
            printf("Sigma Controller ready!\n");
            break;

        case ESP_SPP_SRV_OPEN_EVT:
            client = p->srv_open.handle;
            ready = true;
            printf("Client connected!\n");
            break;

        case ESP_SPP_CLOSE_EVT:
            ready = false;
            client = 0;
            printf("Client disconnected!\n");
            break;

        case ESP_SPP_WRITE_EVT:
            ready = p->write.status == ESP_SPP_SUCCESS && !p->write.cong;
            break;

        case ESP_SPP_CONG_EVT:
            if (!p->cong.cong) ready = true;
            break;

        default:
            break;
    }
}

static void leds_init(void) {
    gpio_config_t out = {
        .pin_bit_mask = (1ULL << PIN_LED_GREEN) | (1ULL << PIN_LED_RED),
        .mode = GPIO_MODE_OUTPUT,
    };
    gpio_config(&out);
    gpio_set_level(PIN_LED_GREEN, 0);
    gpio_set_level(PIN_LED_RED, 0);
}

static void button_init(void) {
    gpio_config_t in = {
        .pin_bit_mask = 1ULL << PIN_BUTTON,
        .mode = GPIO_MODE_INPUT,
        .pull_up_en = GPIO_PULLUP_ENABLE,
    };
    gpio_config(&in);
}

static void stick_init(void) {
    adc_oneshot_unit_init_cfg_t unit = {.unit_id = ADC_UNIT_1};
    ESP_ERROR_CHECK(adc_oneshot_new_unit(&unit, &adc));

    adc_oneshot_chan_cfg_t chan = {
        .atten = ADC_ATTEN_DB_12,
        .bitwidth = ADC_BITWIDTH_12,
    };
    ESP_ERROR_CHECK(adc_oneshot_config_channel(adc, CH_STICK_X, &chan));
    ESP_ERROR_CHECK(adc_oneshot_config_channel(adc, CH_STICK_Y, &chan));
}

static int read_avg(adc_channel_t channel, int samples) {
    int total = 0;
    int raw = 0;
    for (int i = 0; i < samples; i++) {
        adc_oneshot_read(adc, channel, &raw);
        total += raw;
    }
    return total / samples;
}

static void stick_calibrate(void) {
    vTaskDelay(pdMS_TO_TICKS(100));
    center_x = read_avg(CH_STICK_X, 64);
    center_y = read_avg(CH_STICK_Y, 64);
    printf("Stick center x=%d y=%d\n", center_x, center_y);
}

static float axis(int raw, int center, bool invert) {
    float v = (float)(raw - center) / (float)center;
    if (v > 1.0f) v = 1.0f;
    if (v < -1.0f) v = -1.0f;
    if (fabsf(v) < DEADZONE) return 0.0f;
    float sign = v < 0.0f ? -1.0f : 1.0f;
    v = sign * (fabsf(v) - DEADZONE) / (1.0f - DEADZONE);
    return invert ? -v : v;
}

static bool button_pressed(void) {
    static bool stable = false;
    static bool last_raw = false;
    static int64_t changed_at = 0;

    bool raw = gpio_get_level(PIN_BUTTON) == 0;
    int64_t t = now_ms();
    if (raw != last_raw) {
        last_raw = raw;
        changed_at = t;
    }
    if (t - changed_at >= DEBOUNCE_MS) stable = raw;
    return stable;
}

static void lamp_test(void) {
    gpio_set_level(PIN_LED_GREEN, 1);
    gpio_set_level(PIN_LED_RED, 1);
    vTaskDelay(pdMS_TO_TICKS(400));
    gpio_set_level(PIN_LED_GREEN, 0);
    gpio_set_level(PIN_LED_RED, 0);
}

static void update_leds(bool connected, bool fault) {
    int64_t t = now_ms();
    if (fault) {
        gpio_set_level(PIN_LED_GREEN, 0);
        gpio_set_level(PIN_LED_RED, (t / BLINK_FAULT_MS) % 2);
    } else if (connected) {
        gpio_set_level(PIN_LED_GREEN, 1);
        gpio_set_level(PIN_LED_RED, 0);
    } else {
        gpio_set_level(PIN_LED_GREEN, 0);
        gpio_set_level(PIN_LED_RED, (t / BLINK_WAITING_MS) % 2);
    }
}

static esp_err_t bt_start(void) {
    esp_err_t err = nvs_flash_init();
    if (err != ESP_OK) return err;

    esp_bt_controller_config_t cfg = BT_CONTROLLER_INIT_CONFIG_DEFAULT();
    err = esp_bt_controller_init(&cfg);
    if (err != ESP_OK) return err;
    err = esp_bt_controller_enable(cfg.mode);
    if (err != ESP_OK) return err;

    err = esp_bluedroid_init();
    if (err != ESP_OK) return err;
    err = esp_bluedroid_enable();
    if (err != ESP_OK) return err;
    err = esp_spp_register_callback(bt_cb);
    if (err != ESP_OK) return err;

    esp_spp_cfg_t spp = {.mode = ESP_SPP_MODE_CB};
    return esp_spp_enhanced_init(&spp);
}

void app_main(void) {
    leds_init();
    button_init();
    stick_init();
    lamp_test();
    stick_calibrate();

    if (bt_start() != ESP_OK) {
        printf("Bluetooth init failed\n");
        while (1) {
            update_leds(false, true);
            vTaskDelay(pdMS_TO_TICKS(20));
        }
    }

    static char msg[64];
    int tick = 0;
    int64_t interact_until = 0;
    bool was_pressed = false;
    int64_t not_ready_since = 0;

    while (1) {
        int64_t t = now_ms();
        bool connected = client != 0;

        bool pressed = button_pressed();
        if (pressed && !was_pressed) interact_until = t + INTERACT_HOLD_MS;
        was_pressed = pressed;

        if (connected && ready) {
            not_ready_since = 0;

#if MOCK_INPUT
            float moveX = (float)((tick % 21) - 10) / 10.0f;
            float moveY = (float)(((tick + 10) % 21) - 10) / 10.0f;
            int interact = (tick % 20 < 5) ? 1 : 0;
#else
            float moveX = axis(read_avg(CH_STICK_X, 4), center_x, INVERT_X);
            float moveY = axis(read_avg(CH_STICK_Y, 4), center_y, INVERT_Y);
            int interact = (pressed || t < interact_until) ? 1 : 0;
#endif

            int len = snprintf(msg, sizeof(msg),
                "moveX:%.2f,moveY:%.2f,Interact:%d\n",
                moveX, moveY, interact);

            ready = false;

            if (len > 0 && len < (int)sizeof(msg) &&
                esp_spp_write(client, len, (uint8_t *)msg) == ESP_OK) {
                tick++;
            } else {
                ready = true;
            }
        } else if (connected) {
            if (not_ready_since == 0) not_ready_since = t;
        } else {
            not_ready_since = 0;
        }

        bool fault = connected && not_ready_since != 0 &&
                     t - not_ready_since > STALL_FAULT_MS;
        update_leds(connected, fault);

        vTaskDelay(pdMS_TO_TICKS(SEND_PERIOD_MS));
    }
}
