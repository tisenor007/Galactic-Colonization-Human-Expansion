using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyNoise : MonoBehaviour
{

    public static float RemapValue(float value, float initialMin, float initialMax, float outputMin, float outputMax)
    {
        return outputMin + (value - initialMin) * (outputMax - outputMin) / (initialMax - initialMin);
    }

    public static float RemapValue01(float value, float outputMin, float outputMax)
    {
        return outputMin + (value - 0) * (outputMax - outputMin) / (1 - 0);
    }

    public static int RemapValue01ToInt(float value, float outputMin, float outputMax)
    {
        return (int)RemapValue01(value, outputMin, outputMax);
    }

    public static float Redistribution(float noise, NoiseData data)
    {
        return Mathf.Pow(noise * data.redistributionModifier, data.exponent);
    }

    public static float OctavePerlin(float x, float z, NoiseData data)
    {
        x *= data.noiseZoom;
        z *= data.noiseZoom;
        x += data.noiseZoom;
        z += data.noiseZoom;
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float amplitudeSum = 0;
        for (int i = 0; i < data.octaves; i++)
        {
            total += Mathf.PerlinNoise((data.offset.x + data.worldOffset.x + x) * frequency,
                (data.offset.y + data.worldOffset.y + z) * frequency) * amplitude;
            amplitudeSum += amplitude;

            amplitude *= data.persistance;
            frequency *= 2;
        }

        return total / amplitudeSum;
    }
}
