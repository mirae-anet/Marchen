using System.Collections;
using UnityEngine;

namespace OffAxisStudios
{
    public class OASHologramGlitch : MonoBehaviour
    {
        public float glitchChance = 0.025f;
        public float glitchWaitTime = 0.1f;

        Material hologramMat;
        WaitForSeconds glitchWait; 

        void Awake()
        {
            hologramMat = GetComponent<Renderer>().material;
            glitchWait = new WaitForSeconds(glitchWaitTime);
        }

        IEnumerator Start()
        {
            while (true)
            {
                float doGlitch = Random.Range(0f, 1f);

                if (doGlitch <= glitchChance)
                {
                    float originalGlow = hologramMat.GetFloat("_Glow");
                    hologramMat.SetFloat("_GlitchIntensity", Random.Range(0.07f, 0.1f));
                    hologramMat.SetFloat("_Glow", originalGlow * Random.Range(0.14f, 0.44f));
                    yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
                    hologramMat.SetFloat("_GlitchIntensity", 0f);
                    hologramMat.SetFloat("_Glow", originalGlow);
                }

                yield return glitchWait;
            }
        }
    }
}