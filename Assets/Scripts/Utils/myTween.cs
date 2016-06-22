using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class myTween : Singleton<myTween>
{
    static int saturationUniform;
    void Awake()
    {
        saturationUniform = Shader.PropertyToID("_Saturation");
    }
    IEnumerator _SpriteColorFade(GameObject obj, Color targetColor, float time, float delay)
    {
        if(delay > 0.0f)
            yield return new WaitForSeconds(delay);

        int steps = Mathf.Max(1, (int)Mathf.Ceil(time / 0.033f));
        float timeStep = time / (float)steps;

        for (int i = 1; i <= steps; i++)
        {
            float f = (float)i / (float)steps;

            if (obj == null) yield break;
            SpriteRenderer[] allSR = obj.GetComponentsInChildren<SpriteRenderer>();
            if (allSR != null)
            {
                foreach(SpriteRenderer sr in allSR)
                    sr.color = Color.Lerp(sr.color, targetColor, f * f);
            }

            CanvasGroup[] allCG = obj.GetComponentsInChildren<CanvasGroup>();
            if (allCG != null)
            {
                foreach (CanvasGroup cg in allCG)
                {
                    cg.alpha = Mathf.Lerp(cg.alpha, targetColor.a, f * f);
                }
            }

            Text[] allTxt = obj.GetComponentsInChildren<Text>();
            if (allTxt != null)
            {
                foreach (Text tm in allTxt)
                {
                    tm.color = Color.Lerp(tm.color, targetColor, f * f);
                }
            }

            Image[] allImg = obj.GetComponentsInChildren<Image>();
            if (allImg != null)
            {
                foreach (Image img in allImg)
                {
                    img.color = Color.Lerp(img.color, targetColor, f * f);
                }
            }

            if(timeStep > 0.0f)
                yield return new WaitForSeconds(timeStep);
        }

        yield return null;
    }

    void _SpriteColorFadeDirect(GameObject obj, Color targetColor)
    {
        if (obj == null)
            return;
        SpriteRenderer[] allSR = obj.GetComponentsInChildren<SpriteRenderer>();
        if (allSR != null)
        {
            foreach (SpriteRenderer sr in allSR)
                sr.color = targetColor;
        }

        CanvasGroup[] allCG = obj.GetComponentsInChildren<CanvasGroup>();
        if (allCG != null)
        {
            foreach (CanvasGroup cg in allCG)
            {
                cg.alpha = targetColor.a;
            }
        }

        Text[] allTxt = obj.GetComponentsInChildren<Text>();
        if (allTxt != null)
        {
            foreach (Text tm in allTxt)
            {
                tm.color = targetColor;
            }
        }

        Image[] allImg = obj.GetComponentsInChildren<Image>();
        if (allImg != null)
        {
            foreach (Image img in allImg)
            {
                img.color = targetColor;
            }
        }
    }

    public void SpriteColorFade(GameObject obj, Color col, float time, float delay=0.0f)
    {
        if(time == 0.0f && delay == 0.0f)
        {
            _SpriteColorFadeDirect(obj, col);
            return;
        }
        StopCoroutine(_SpriteColorFade(obj, col, time, delay));
        StartCoroutine(_SpriteColorFade(obj, col, time, delay));
    }

    IEnumerator _CanvasGroupAlphaFade(GameObject obj, float targetAlpha, float time, float delay)
    {
        if (delay > 0.0f)
            yield return new WaitForSeconds(delay);

        int steps = Mathf.Max(1, (int)Mathf.Ceil(time / 0.033f));
        float timeStep = time / (float)steps;

        for (int i = 1; i <= steps; i++)
        {
            float f = (float)i / (float)steps;

            if (obj == null) yield break;

            CanvasGroup[] allCG = obj.GetComponentsInChildren<CanvasGroup>();
            if (allCG != null)
            {
                foreach (CanvasGroup cg in allCG)
                {
                    cg.alpha = Mathf.Lerp(cg.alpha, targetAlpha, f * f);
                }
            }

            if (timeStep > 0.0f)
                yield return new WaitForSeconds(timeStep);
        }

        yield return null;
    }

    public void CanvasGroupAlphaFade(GameObject obj, float targetAlpha, float time, float delay = 0.0f)
    {
        StopCoroutine(_CanvasGroupAlphaFade(obj, targetAlpha, time, delay));
        StartCoroutine(_CanvasGroupAlphaFade(obj, targetAlpha, time, delay));
    }

    IEnumerator _SpriteAlphaFade(GameObject obj, float targetAlpha, float time, float delay)
    {
        if(delay > 0.0f)
            yield return new WaitForSeconds(delay);

        int steps = Mathf.Max(1, (int)Mathf.Ceil(time / 0.033f));
        float timeStep = time / (float)steps;

        for (int i = 1; i <= steps; i++)
        {
            float f = (float)i / (float)steps;

            if (obj == null) yield break;
            SpriteRenderer[] allSR = obj.GetComponentsInChildren<SpriteRenderer>();
            if (allSR != null)
            {
                foreach (SpriteRenderer sr in allSR)
                {
                    Color c = sr.color;
                    c.a = Mathf.Lerp(c.a, targetAlpha, f * f);
                    sr.color = c;
                }
            }

            TextMesh[] allTM = obj.GetComponentsInChildren<TextMesh>();
            if (allTM != null)
            {
                foreach (TextMesh tm in allTM)
                {
                    Color c = tm.color;
                    c.a = Mathf.Lerp(c.a, targetAlpha, f * f);
                    tm.color = c;
                }
            }

            Text[] allTxt = obj.GetComponentsInChildren<Text>();
            if (allTxt != null)
            {
                foreach (Text tm in allTxt)
                {
                    Color c = tm.color;
                    c.a = Mathf.Lerp(c.a, targetAlpha, f * f);
                    tm.color = c;
                }
            }

            Image[] allImg = obj.GetComponentsInChildren<Image>();
            if (allImg != null)
            {
                foreach (Image img in allImg)
                {
                    Color c = img.color;
                    c.a = Mathf.Lerp(c.a, targetAlpha, f * f);
                    img.color = c;
                }
            }

            CanvasGroup[] allCG = obj.GetComponentsInChildren<CanvasGroup>();
            if (allCG != null)
            {
                foreach (CanvasGroup cg in allCG)
                {
                    cg.alpha = Mathf.Lerp(cg.alpha, targetAlpha, f);
                }
            }

            if(timeStep > 0.0f)
                yield return new WaitForSeconds(timeStep);
        }

        yield return null;
    }

    void _SpriteAlphaFadeDirect(GameObject obj, float targetAlpha)
    {
        if (obj == null) return;
        SpriteRenderer[] allSR = obj.GetComponentsInChildren<SpriteRenderer>();
        if (allSR != null)
        {
            foreach (SpriteRenderer sr in allSR)
            {
                Color c = sr.color;
                c.a = targetAlpha;
                sr.color = c;
            }
        }

        TextMesh[] allTM = obj.GetComponentsInChildren<TextMesh>();
        if (allTM != null)
        {
            foreach (TextMesh tm in allTM)
            {
                Color c = tm.color;
                c.a = targetAlpha;
                tm.color = c;
            }
        }

        Text[] allTxt = obj.GetComponentsInChildren<Text>();
        if (allTxt != null)
        {
            foreach (Text tm in allTxt)
            {
                Color c = tm.color;
                c.a = targetAlpha;
                tm.color = c;
            }
        }

        Image[] allImg = obj.GetComponentsInChildren<Image>();
        if (allImg != null)
        {
            foreach (Image img in allImg)
            {
                Color c = img.color;
                c.a = targetAlpha;
                img.color = c;
            }
        }

        CanvasGroup[] allCG = obj.GetComponentsInChildren<CanvasGroup>();
        if (allCG != null)
        {
            foreach (CanvasGroup cg in allCG)
            {
                cg.alpha = targetAlpha;
            }
        }
    }

    public void SpriteAlphaFade(GameObject obj, float alpha, float time, float delay = 0.0f)
    {
        if(time == 0.0f && delay == 0.0f)
        {
            _SpriteAlphaFadeDirect(obj, alpha);
            return;
        }
        StopCoroutine(_SpriteAlphaFade(obj, alpha, time, delay));
        StartCoroutine(_SpriteAlphaFade(obj, alpha, time, delay));
    }

    IEnumerator _ImageColorFade(GameObject obj, Color targetColor, float time)
    {
        int steps = Mathf.Max(1, (int)Mathf.Ceil(time / 0.033f));
        float timeStep = time / (float)steps;

        for (int i = 1; i <= steps; i++)
        {
            float f = (float)i / (float)steps;

            if (obj == null) yield break;
            UnityEngine.UI.Image[] allImg = obj.GetComponentsInChildren<UnityEngine.UI.Image>();
            if (allImg != null)
            {
                foreach (UnityEngine.UI.Image img in allImg)
                {
                    if(img.material != null)
                        img.material.color = Color.Lerp(img.material.color, targetColor, f * f);
                    else
                        img.color = Color.Lerp(img.color, targetColor, f * f);
                }
            }

            yield return new WaitForSeconds(timeStep);
        }
    }

    void _ImageColorFadeDirect(GameObject obj, Color targetColor)
    {
        if (obj == null) return;
        UnityEngine.UI.Image[] allImg = obj.GetComponentsInChildren<UnityEngine.UI.Image>();
        if (allImg != null)
        {
            foreach (UnityEngine.UI.Image img in allImg)
            {
                if (img.material != null)
                    img.material.color = targetColor;
                else
                    img.color = targetColor;
            }
        }
    }

    public void ImageColorFade(GameObject obj, Color col, float time)
    {
        if(time == 0.0f)
        {
            _ImageColorFadeDirect(obj, col);
            return;
        }
        StopCoroutine(_ImageColorFade(obj, col, time));
        StartCoroutine(_ImageColorFade(obj, col, time));
    }

    IEnumerator _FadeSaturation(GameObject obj, float newSaturation, float time, float delay)
    {
        if(delay > 0.0f)
            yield return new WaitForSeconds(delay);

        int steps = Mathf.Max(1, (int)Mathf.Ceil(time / 0.033f));
        float timeStep = time / (float)steps;

        for (int i = 1; i <= steps; i++)
        {
            if (obj != null)
            {
                Image[] cr = obj.GetComponentsInChildren<Image>();
                float f = (float)i / (float)steps;
                f = Mathf.Pow(f, 3.0f);
                for (int j = 0; j < cr.Length; j++)
                {
                    Material m = cr[j].material;
                    if (m != null && m.HasProperty(saturationUniform))
                        m.SetFloat(saturationUniform, Mathf.Lerp(m.GetFloat(saturationUniform), newSaturation, f));
                }

                RawImage[] ris = obj.GetComponentsInChildren<RawImage>();
                for (int j = 0; j < ris.Length; j++)
                {
                    Material m = ris[j].material;
                    if (m != null && m.HasProperty(saturationUniform))
                        m.SetFloat(saturationUniform, Mathf.Lerp(m.GetFloat(saturationUniform), newSaturation, f));
                }

                SpriteRenderer[] sr = obj.GetComponentsInChildren<SpriteRenderer>();
                for (int j = 0; j < sr.Length; j++)
                {
                    Material m = sr[j].material;
                    if (m != null && m.HasProperty(saturationUniform))
                        m.SetFloat(saturationUniform, Mathf.Lerp(m.GetFloat(saturationUniform), newSaturation, f));
                }

                /*ParticleSystem[] ps = obj.GetComponentsInChildren<ParticleSystem>();
                for (int j = 0; j < ps.Length; j++)
                {
                    Material m = ps[j].renderer.material;
                    if (m != null && m.HasProperty("_Saturation"))
                        m.SetFloat("_Saturation", Mathf.Lerp(m.GetFloat("_Saturation"), newSaturation, f));
                }*/
            }

            if(timeStep > 0.0f)
                yield return new WaitForSeconds(timeStep);
        }

        yield return null;
    }

    void _FadeSaturationDirect(GameObject obj, float newSaturation)
    {
        Image[] cr = obj.GetComponentsInChildren<Image>();
        if (cr == null)
            return;

        for (int i = 0; i < cr.Length; i++)
        {
            Material m = cr[i].material;
            if (m == null) continue;
            if (m.HasProperty("_Saturation"))
            {
                m.SetFloat("_Saturation", newSaturation);
            }
        }

        SpriteRenderer[] sr = obj.GetComponentsInChildren<SpriteRenderer>();
        if (sr == null)
            return;

        for (int i = 0; i < sr.Length; i++)
        {
            Material m = sr[i].material;
            if (m == null) continue;
            if (m.HasProperty("_Saturation"))
            {
                m.SetFloat("_Saturation", newSaturation);
            }
        }

        /*ParticleSystem[] ps = obj.GetComponentsInChildren<ParticleSystem>();
        if (ps == null)
            return;

        for (int i = 0; i < ps.Length; i++)
        {
            Material m = ps[i].renderer.material;
            if (m == null) continue;
            if (m.HasProperty("_Saturation"))
            {
                m.SetFloat("_Saturation", newSaturation);
            }
        }*/
    }

    public void FadeSaturation(GameObject obj, float newSaturation, float time, float delay = 0.0f)
    {
        if(time == 0.0f && delay == 0.0f)
        {
            _FadeSaturationDirect(obj, newSaturation);
            return;
        }
        StopCoroutine(_FadeSaturation(obj, newSaturation, time, delay));
        StartCoroutine(_FadeSaturation(obj, newSaturation, time, delay));
    }

    IEnumerator _FadeEmissive(GameObject obj, Vector3 newEmissive, float time, float delay)
    {
        if (delay > 0.0f)
            yield return new WaitForSeconds(delay);

        int steps = Mathf.Max(1, (int)Mathf.Ceil(time / 0.033f));
        float timeStep = time / (float)steps;

        Renderer[] ren = obj.GetComponentsInChildren<Renderer>();
        if (ren == null)
            yield break;

        bool immediate = time < 0.0001f;

        Vector3[] initialEmissive = new Vector3[ren.Length];
        for (int i = 0; i < ren.Length; i++)
        {
            if (ren[i] == null) continue;
            Material m = ren[i].material;
            if (m == null) continue;
            if (m.HasProperty("_EmissiveColor"))
            {
                initialEmissive[i] = m.GetVector("_EmissiveColor");
                if (immediate)
                {
                    m.SetVector("_EmissiveColor", newEmissive);
                }
            }
            else
                initialEmissive[i] = -Vector3.one;
        }

        if (immediate)
            yield break;

        for (int i = 1; i <= steps; i++)
        {
            float f = (float)i / (float)steps;
            for (int j = 0; j < ren.Length; j++)
            {
                if (initialEmissive[j].x < 0.0f) continue;

                if (ren[j] == null) continue;
                Material m = ren[j].material;
                if (m != null)
                    m.SetVector("_EmissiveColor", Vector3.Lerp(initialEmissive[j], newEmissive, f * f));
            }

            if (timeStep > 0.0f)
                yield return new WaitForSeconds(timeStep);
        }

        yield return null;
    }

    public void FadeEmissive(GameObject obj, Vector3 newEmissive, float time, float delay = 0.0f)
    {
        StopCoroutine(_FadeEmissive(obj, newEmissive, time, delay));
        StartCoroutine(_FadeEmissive(obj, newEmissive, time, delay));
    }


    IEnumerator _Activate(GameObject obj, bool activate, float delay)
    {
        if(delay >= 0.0f)
            yield return new WaitForSeconds(delay);

        if (obj != null)
            obj.SetActive(activate);
    }

    IEnumerator _ActivateFade(GameObject obj, bool activate, float delay, bool isAdditive)
    {
        float fadeTime = 0.7f;
        if(delay < 0.01f)
        {
            //fadeTime = 0.0f;
        }

        if(activate == false)
        {
            if(delay > 0.0f)
                yield return new WaitForSeconds(delay - fadeTime);
            if (isAdditive)
                SpriteColorFade(obj, Color.black, fadeTime);
            else
                SpriteAlphaFade(obj, 0.0f, fadeTime);
            if (delay > 0.0f)
                yield return new WaitForSeconds(fadeTime);
        }
        else
        {
            if (delay > 0.0f)
                yield return new WaitForSeconds(delay);
        }

        if (obj == null)
            yield break;

        obj.SetActive(activate);

        if(activate)
        {
            if (isAdditive)
            {
                SpriteColorFade(obj, Color.white, fadeTime);
            }
            else
            {
                SpriteAlphaFade(obj, 0.0f, 0.0001f);
                SpriteAlphaFade(obj, 1.0f, fadeTime);
            }
        }

        yield return null;
    }

    public void Activate(GameObject obj, bool activate = true, float delay = 0.0f)
    {
        if (obj == null) return;
        if(delay == 0.0f)
        {
            obj.SetActive(activate);
            return;
        }

        StartCoroutine(_Activate(obj, activate, delay));
    }
    public void ActivateFade(GameObject obj, bool activate = true, float delay = 0.0f, bool isAdditive = false)
    {
        if (obj.activeSelf == activate) return;

        if (activate)
        {
            StartCoroutine(_ActivateFade(obj, activate, delay, isAdditive));
            ActivateParticleSystem(obj, activate, delay * 1.1f);
        }
        else
        {
            ActivateParticleSystem(obj, activate, 0.0f, 0.5f);
            if (activate && delay == 0.0f)
                obj.SetActive(true);
            StartCoroutine(_ActivateFade(obj, activate, delay, isAdditive));
        }
    }

    IEnumerator _ActivateParticleSystem(GameObject obj, bool activate, float delay, float duration)
    {
        yield return new WaitForSeconds(delay);
        if (obj == null)
            yield break;
        ParticleSystem[] allPS = obj.GetComponentsInChildren<ParticleSystem>();
        if(allPS != null)
        {
            foreach (ParticleSystem ps in allPS)
            {
                if (activate)
                {
                    ps.enableEmission = true;
                    ps.Play();
                }
                else
                {
                    ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
                    int count = ps.GetParticles(particles);

                    for (int i = 0; i < count; i++ )
                    {
                        particles[i].lifetime = Mathf.Min(duration, particles[i].lifetime);
                    }

                    ps.SetParticles(particles, count);
                    ps.enableEmission = false;
                    ps.Stop();
                }
            }
        }
    }

    public void ActivateParticleSystem(GameObject obj, bool activate = true, float delay = 0.0f, float duration = 0.5f)
    {
        StartCoroutine(_ActivateParticleSystem(obj, activate, delay, duration));
    }

    public void ActivateParticleSystemDirect(GameObject obj, bool activate = true)
    {
        if (obj == null) return;
        ParticleSystem[] allPS = obj.GetComponentsInChildren<ParticleSystem>();
        if (allPS != null)
        {
            foreach(ParticleSystem ps in allPS)
            {
                if (activate)
                {
                    ps.Stop();
                    ps.Play();
                }
                else
                    ps.Stop();
            }
        }
    }

    IEnumerator _FadeCanvasGroup(GameObject obj, float newAlpha, float duration = 0.7f, float delay = 0.0f)
    {
        if(delay > 0.0f)
            yield return new WaitForSeconds(delay);

        int steps = Mathf.Max(1, (int)Mathf.Ceil(duration / 0.033f));
        float timeStep = duration / (float)steps;

        for (int i = 1; i <= steps; i++)
        {
            float f = (float)i / (float)steps;

            if (obj == null) yield break;

            CanvasGroup[] allCG = obj.GetComponentsInChildren<CanvasGroup>();
            if (allCG != null)
            {
                foreach (CanvasGroup cg in allCG)
                {
                    cg.alpha = Mathf.Lerp(cg.alpha, newAlpha, f * f);
                }
            }

            if (timeStep > 0.0f)
                yield return new WaitForSeconds(timeStep);
        }

        yield return null;
    }

    public void FadeCanvasGroup(GameObject obj, float newAlpha, float duration = 0.7f, float delay = 0.0f)
    {
        if(delay == 0.0f && duration == 0.0f)
        {
            CanvasGroup[] allCG = obj.GetComponentsInChildren<CanvasGroup>();
            if (allCG != null)
            {
                foreach (CanvasGroup cg in allCG)
                {
                    cg.alpha = newAlpha;
                }
            }
        }
        else
            StartCoroutine(_FadeCanvasGroup(obj, newAlpha, duration, delay));
    }

    IEnumerator _FadeFov(GameObject obj, float targetFov, float duration, float delay = 0.0f)
    {
        yield return new WaitForSeconds(delay);

        if (delay > 0.0f)
            yield return new WaitForSeconds(delay);

        int steps = Mathf.Max(1, (int)Mathf.Ceil(duration / 0.033f));
        float timeStep = duration / (float)steps;

        for (int i = 1; i <= steps; i++)
        {
            float f = (float)i / (float)steps;

            if (obj == null) yield break;

            Camera cam = obj.GetComponent<Camera>();
            if (cam == null) yield break;

            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, f * f);

            if (timeStep > 0.0f)
                yield return new WaitForSeconds(timeStep);
        }

        yield return null;
    }

    public void FadeFov(GameObject obj, float targetFov, float duration, float delay = 0.0f)
    {
        StartCoroutine(_FadeFov(obj, targetFov, duration, delay));
    }
}
