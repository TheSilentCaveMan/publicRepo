using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class ItemPickupable : ItemBase, IGUIData
{
    public int _amount;
    public string itemType;
    public ParticleSystem[] itemParticleSystem;
    public AudioSource[] audioPackage;
    public Animation[] animationPackage;
    private Material outline;
    private Renderer outlineRenderer;
    private float outlineTimer;
    private const float outlineDelay = 0.1f;
    internal bool outlineIsOn = false;
    private void Awake()
    {
        outlineRenderer = GetComponent<Renderer>();
        outline = Instantiate(Resources.Load<Material>("Graphics/Materials/OutlineShaderGraph_Material"));
        var materials = outlineRenderer.sharedMaterials.ToList();
        materials.Add(outline);
        outlineRenderer.materials = materials.ToArray();
    }
    private void Update()
    {
        ManageOutline();
    }
    private void ManageOutline()
    {
        outlineTimer += Time.deltaTime;
        if (outlineTimer > outlineDelay && outline != null)
        {
            if (outlineIsOn)
            {
                outline.SetFloat("_Scale", 1.02f);
                outlineIsOn = false;
            }
            else
            {
                outline.SetFloat("_Scale", 0f);
            }
            outlineTimer = 0;
        }
    }
    internal void PlayAudio(AudioSource source)
    {
        source.Play();
    }
    internal void StopAudio(AudioSource source)
    {
        source.Stop();
    }
    internal void PlayAnimation(Animation source)
    {
        source.Play();
    }
    internal void StopAnimation(Animation source)
    {
        source.Stop();
    }
    internal void PlayParticles(ParticleSystem source)
    {
        source.Play();
    }
    internal void StopParticles(ParticleSystem source)
    {
        source.Stop();
    }
}
