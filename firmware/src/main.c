
#include <stdio.h>
#include <stdint.h>
#include <stdbool.h>

#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include "esp_bt.h"
#include "esp_bt_main.h"
#include "esp_gap_bt_api.h"
#include "esp_spp_api.h"
#include "nvs_flash.h"

static uint32_t client = 0;
static volatile bool ready = false;

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

void app_main(void) {
    ESP_ERROR_CHECK(nvs_flash_init());

    esp_bt_controller_config_t cfg = BT_CONTROLLER_INIT_CONFIG_DEFAULT();
    ESP_ERROR_CHECK(esp_bt_controller_init(&cfg));
    ESP_ERROR_CHECK(esp_bt_controller_enable(cfg.mode));

    ESP_ERROR_CHECK(esp_bluedroid_init());
    ESP_ERROR_CHECK(esp_bluedroid_enable());
    ESP_ERROR_CHECK(esp_spp_register_callback(bt_cb));

    esp_spp_cfg_t spp = {.mode = ESP_SPP_MODE_CB};
    ESP_ERROR_CHECK(esp_spp_enhanced_init(&spp));

    static char msg[64];
    int tick = 0;

    while (1) {
        if (client && ready) {

            float moveX = (float)((tick % 21) - 10) / 10.0f;
            float moveY = (float)(((tick + 10) % 21) - 10) / 10.0f;
            int interact = (tick % 20 < 5) ? 1 : 0;

            int len = snprintf(msg, sizeof(msg),
                "moveX:%.2f,moveY:%.2f,Interact:%d\n",
                moveX, moveY, interact);

            ready = false;

            if (len > 0 && len < sizeof(msg) &&
                esp_spp_write(client, len, (uint8_t *)msg) == ESP_OK) {
                tick++;
            } else {
                ready = true;
            }
        }

        vTaskDelay(pdMS_TO_TICKS(100));
    }
}

