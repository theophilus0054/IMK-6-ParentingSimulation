using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TemperatureDisplay : MonoBehaviour
{

    public TextMeshProUGUI temperatureText;
    public List<Material> thermometerMaterials; // List of materials for different temperature ranges

    public void UpdateTemperature(float temperature)
    {
        temperatureText.text = temperature.ToString("F1") + "°C";
        Console.WriteLine($"Temperature: {temperature}°C");

        // Update thermometer material based on temperature
        UpdateThermometerMaterial(temperature);
    }

    public void activateTemperatureText(bool isActive)
    {
        temperatureText.gameObject.SetActive(isActive);
    }

    public void deactivateTemperatureText(bool isActive)
    {
        temperatureText.gameObject.SetActive(!isActive);
    }

    private void UpdateThermometerMaterial(float temperature)
    {
        Material selectedMaterial = null;

        if (temperatureText.isActiveAndEnabled)
        {
            if (temperature <= 37.0f)
            {
                selectedMaterial = thermometerMaterials[1]; // Normal material
            }
            else
            {
                selectedMaterial = thermometerMaterials[2]; // Hot material
            }
        }
        else
        {
            selectedMaterial = thermometerMaterials[0]; // Default material when text is not active
        }

        // Apply the selected material to the thermometer
        GetComponent<Renderer>().material = selectedMaterial;
    }
}
