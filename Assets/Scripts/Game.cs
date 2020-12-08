using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public Transform[] prefabs;

    public KeyCode createKey = KeyCode.C;

	void Update()
	{
		if (Input.GetKeyDown(createKey))
		{
			CreateObject();
		}
	}

	void CreateObject()
	{
		int nRandomIdx = Random.Range(0, prefabs.Length);
		Transform t = Instantiate(prefabs[0]);
		t.localPosition = Random.insideUnitSphere * 5f;
		t.localRotation = Random.rotation;
		t.localScale = Vector3.one * Random.Range(0.1f, 1f);
	}
}
