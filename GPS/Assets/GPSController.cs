using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class GPSController : MonoBehaviour
{
    string message = "Initialising GPS...";
    float thisLat;
    float thisLong;
    float startingLat;
    float startingLong;

    DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public float accuracy = 5f;
    public float displacement = 0f;
    public GameObject needle;

    public static float heading;
    public static float distance;

    void Start()
    {
        StartCoroutine(StartGPS());
        InvokeRepeating("CustomUpdate", 1, 1);
    }

    void CustomUpdate()
    {
        DateTime lastUpdate = epoch.AddSeconds(Input.location.lastData.timestamp);
        DateTime rightNow = DateTime.Now;

        thisLat = Input.location.lastData.latitude;
        thisLong = Input.location.lastData.longitude;

        distance = Haversine(thisLat, thisLong, startingLat, startingLong);

        message =
            "Distance Travelled: " + distance +
            "\nHeading: " + Input.compass.trueHeading +
            "\nUpdate Time: " + lastUpdate.ToString("HH:mm:ss") +
            "\nNow: " + lastUpdate.ToString("HH:mm:ss");

        heading = Input.compass.trueHeading;
        needle.transform.localRotation = Quaternion.Euler(0, 0, heading);
    }

    public void Reset()
    {
        startingLat = Input.location.lastData.latitude;
        startingLong = Input.location.lastData.longitude;
    }

    float Haversine(float lat1, float long1, float lat2, float long2)
    {
        float earthRad = 6371000;
        float lRad1 = lat1 * Mathf.Deg2Rad;
        float lRad2 = lat2 * Mathf.Deg2Rad;
        float dLat = (lat2 - lat1) * Mathf.Deg2Rad;
        float dLong = (long2 - long1) * Mathf.Deg2Rad;
        float a = Mathf.Sin(dLat / 2f) * Mathf.Sin(dLat / 2f) +
            Mathf.Cos(lRad1) * Mathf.Cos(lRad2) *
            Mathf.Sin(dLong / 2f) * Mathf.Sin(dLong / 2f);
        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));

        return earthRad * c;
    }

    IEnumerator StartGPS()
    {
        message = "Starting";

        if (!Input.location.isEnabledByUser)
        {
            message = "Location Services Not Enabled";
            yield break;
        }

        Input.location.Start(accuracy, displacement);

        int maxWait = 5;

        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1)
        {
            message = "Timed out";
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            message = "Unable to determine device location";
            yield break;
        }
        else
        {
            Input.compass.enabled = true;
            message =
                "Lat: " + Input.location.lastData.latitude +
                "\nLong: " + Input.location.lastData.longitude +
                "\nAlt: " + Input.location.lastData.altitude +
                "\nHoriz Acc: " + Input.location.lastData.horizontalAccuracy +
                "\nVert Acc: " + Input.location.lastData.verticalAccuracy +
                "\n========" +
                "\nHeading: " + Input.compass.trueHeading;

            startingLat = Input.location.lastData.latitude;
            startingLong = Input.location.lastData.longitude;
        }

        //Input.location.Stop();
    }

    void OnGUI()
    {
        GUI.skin.label.fontSize = 50;
        GUI.Label(new Rect(30, 30, 1000, 1000), message);
    }
}
