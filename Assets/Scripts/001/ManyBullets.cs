using System.Runtime.InteropServices;
using UnityEngine;

struct Bullet {
  public Vector3 pos;

  public Vector3 accel;

  public Color color;

  public int state;

  public Bullet (Vector3 pos, Vector3 accel, Color color) {
    this.pos = pos;
    this.accel = accel;
    this.color = color;
    this.state = 1;
  }
}

public class ManyBullets : MonoBehaviour {

  public Shader bulletsShader;

  public Texture bulletsTexture;

  public ComputeShader bulletsComputeShader;

  Material bulletsMaterial;

  ComputeBuffer bulletsBuffer;

  private Bullet[] bullets;

  private int frameCount;

  void OnDisable () {
    // コンピュートバッファは明示的に破棄しないと怒られます
    bulletsBuffer.Release ();
  }

  void Start () {
    bulletsMaterial = new Material (bulletsShader);
    InitializeComputeBuffer ();
    this.frameCount = 0;
  }

  void Update () {
    bulletsBuffer.GetData (this.bullets);

    // 
    this.bullets[this.frameCount++] = new Bullet (
      new Vector3 (0, 0, 0),
      new Vector3 (Mathf.Cos (this.frameCount / 100.0f), 0, Mathf.Sin (this.frameCount / 100.0f)),
      Color.HSVToRGB (1, 0.8f, 0.5f)
    );

    bulletsBuffer.SetData (this.bullets);

    bulletsComputeShader.SetBuffer (0, "Bullets", bulletsBuffer);
    bulletsComputeShader.SetFloat ("DeltaTime", Time.deltaTime);
    bulletsComputeShader.Dispatch (0, bulletsBuffer.count / 8 + 1, 1, 1);
  }

  void InitializeComputeBuffer () {
    bulletsBuffer = new ComputeBuffer (1000, Marshal.SizeOf (typeof (Bullet)));

    this.bullets = new Bullet[bulletsBuffer.count];
    for (int i = 0; i < bulletsBuffer.count; i++) {
      // bullets[i] =
      //   new Bullet (
      //     new Vector3 (0, 0, 0),
      //     new Vector3 (Mathf.Cos (Mathf.PI * 2 * i / 1000.0f), 0, Mathf.Sin (Mathf.PI * 2 * i / 1000.0f)),
      //     Color.HSVToRGB (i / 1000.0f, 0.8f, 0.5f)
      //   );
      this.bullets[i] = new Bullet ();
    }

    bulletsBuffer.SetData (this.bullets);
  }

  void OnRenderObject () {
    bulletsMaterial.SetTexture ("_MainTex", bulletsTexture);
    bulletsMaterial.SetBuffer ("Bullets", bulletsBuffer);

    bulletsMaterial.SetPass (0);

    Graphics.DrawProceduralNow (MeshTopology.Points, bulletsBuffer.count);
  }

}
