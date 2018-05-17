using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spinner : MonoBehaviour {

	public Vector3 m_rotation;

	void Update () {
		transform.Rotate(m_rotation * Time.deltaTime);		
	}
}
