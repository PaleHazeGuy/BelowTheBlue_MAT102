using System;
using System.Globalization;
using UnityEngine;

namespace BelowTheBlue.Input
{
    public static class ControllerLineParser
    {
        public static bool TryParse(string line, out PlayerInputState state)
        {
            state = PlayerInputState.Neutral;
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            bool hasX = false;
            bool hasY = false;
            float x = 0f;
            float y = 0f;
            bool interact = false;

            string[] fields = line.Trim().Split(',');
            foreach (string field in fields)
            {
                int colon = field.IndexOf(':');
                if (colon <= 0)
                {
                    return false;
                }

                string key = field.Substring(0, colon).Trim();
                string value = field.Substring(colon + 1).Trim();

                if (key.Equals("moveX", StringComparison.OrdinalIgnoreCase))
                {
                    hasX = TryFloat(value, out x);
                    if (!hasX)
                    {
                        return false;
                    }
                }
                else if (key.Equals("moveY", StringComparison.OrdinalIgnoreCase))
                {
                    hasY = TryFloat(value, out y);
                    if (!hasY)
                    {
                        return false;
                    }
                }
                else if (key.Equals("Interact", StringComparison.OrdinalIgnoreCase))
                {
                    if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int flag))
                    {
                        return false;
                    }

                    interact = flag != 0;
                }
            }

            if (!hasX || !hasY)
            {
                return false;
            }

            state = new PlayerInputState(new Vector2(x, y), interact);
            return true;
        }

        private static bool TryFloat(string value, out float result)
        {
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result)
                && !float.IsNaN(result)
                && !float.IsInfinity(result);
        }
    }
}
