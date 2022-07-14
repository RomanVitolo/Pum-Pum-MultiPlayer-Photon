using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using  Photon.Realtime;

public class Resource : MonoBehaviour
{
    [field: SerializeField]
    public ResourcesDataSO ResourcesData { get; set; }

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    
    public void PickUpResource()
    {
        StartCoroutine(DestroyCoroutine());
    }
    
    IEnumerator DestroyCoroutine()
    {
        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        _audioSource.Play();
        yield return new WaitForSeconds(_audioSource.clip.length);
        Destroy(gameObject);
        // PhotonNetwork.Destroy(gameObject) ?? 
        // La reproduccion del Audio deberia estar en el servidor?
    }
}
