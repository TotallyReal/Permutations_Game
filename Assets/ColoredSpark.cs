using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ColoredSpark : MonoBehaviour
{
    private ParticleSystem particles;
    [SerializeField] private CogLink link;

    private void Awake()
    {
        particles = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        link.OnLinked += OnLinked;
    }

    private void OnDisable()
    {
        link.OnLinked -= OnLinked;
    }

    private void OnLinked(object sender, (int, int) teeth)
    {
        ParticleSystem.MainModule main = particles.main;
        if (teeth == (0, 0))
        {
            main.startColor = Color.yellow;
        } else
        {
            main.startColor = Color.red;
        }
        particles.Play();
    }

}
