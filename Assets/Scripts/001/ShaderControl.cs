using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderControl : MonoBehaviour {

  [SerializeField]
  private ComputeShader computeShader;

  [SerializeField]
  private Transform cube;

  private ComputeBuffer buffer;

  // Start is called before the first frame update
  void Start () {
    this.buffer = new ComputeBuffer (1, sizeof (float));
    this.computeShader.SetBuffer (0, "Result", this.buffer);
  }

  // Update is called once per frame
  void Update () {
    this.computeShader.SetFloat ("positionX", this.cube.position.x);
    this.computeShader.Dispatch (0, 8, 8, 1);

    var data = new float[1];
    this.buffer.GetData (data);

    float positionX = data[0];

    Vector3 boxPosition = this.cube.position;
    boxPosition.x = positionX;
    this.cube.position = boxPosition;
  }

  private void OnDestroy () {
    this.buffer.Release ();
  }
}
