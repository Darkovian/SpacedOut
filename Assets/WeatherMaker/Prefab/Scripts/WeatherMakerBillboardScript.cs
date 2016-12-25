using UnityEngine;
using System.Collections;

namespace DigitalRuby.WeatherMaker
{
    public class WeatherMakerBillboardScript : MonoBehaviour
    {
        private void DoBillboard()
        {
            transform.LookAt(transform.position + (Camera.main.transform.rotation * Vector3.forward),
                Camera.main.transform.rotation * Vector3.up);
        }

        private void Start()
        {
            DoBillboard();
        }

        private void Update()
        {
            DoBillboard();
        }
    }
}