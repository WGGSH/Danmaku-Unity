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

public class DanmakuController : MonoBehaviour {

  public Shader bulletsShader;

  public Texture bulletsTexture;

  public ComputeShader bulletsComputeShader;

  Material bulletsMaterial;

  ComputeBuffer bulletsBuffer;

  private Bullet[] bullets;

  private int frameCount;

  private int lastBulletIndex;

  [SerializeField]
  private int BULLET_MAX;

  void OnDisable () {
    // コンピュートバッファは明示的に破棄しないと怒られます
    bulletsBuffer.Release ();
  }

  void Start () {
    bulletsMaterial = new Material (bulletsShader);
    InitializeComputeBuffer ();
    this.frameCount = 0;
    this.lastBulletIndex = 0;
  }

  void Update () {
    bulletsBuffer.GetData (this.bullets);

    //
    this.frameCount++;
    if (frameCount % 60 == 0) {
      float color = Random.Range (0.0f, 1.0f);
      Vector3 pos = new Vector3 (
        Random.Range (-10, 10), Random.Range (-10, 10), Random.Range (-10, 10)
      );
      const int X = 32;
      const int Y = 16;
      for (int x = 0; x < 128; x++) {
        for (int y = 0; y < 64; y++) {
          this.createBullet (
            new Bullet (
              pos,
              new Vector3 (
                Mathf.Cos (Mathf.PI * 2 / X * x) * Mathf.Cos (Mathf.PI * 1 / Y * y),
                Mathf.Sin (Mathf.PI * 1 / Y * y),
                Mathf.Sin (Mathf.PI * 2 / X * x) * Mathf.Cos (Mathf.PI * 1 / Y * y)
              ),
              Color.HSVToRGB (color, 0.9f, 0.5f)
            )
          );
        }
      }
    }
    // this.createBullet (
    //   new Bullet (
    //     Vector3.zero,
    //     new Vector3 (
    //       Mathf.Cos (this.frameCount / 20.0f),
    //       0,
    //       Mathf.Sin (this.frameCount / 20.0f)
    //     ),
    //     Color.HSVToRGB ((this.frameCount / 50.0f) % 1.0f, 0.8f, 0.5f)
    //   )
    // );
    // this.bullets[this.frameCount++] = new Bullet (
    //   new Vector3 (0, 0, 0),
    //   new Vector3 (Mathf.Cos (this.frameCount / 100.0f), 0, Mathf.Sin (this.frameCount / 100.0f)),
    //   Color.HSVToRGB (1, 0.8f, 0.5f)
    // );

    bulletsBuffer.SetData (this.bullets);

    bulletsComputeShader.SetBuffer (0, "Bullets", bulletsBuffer);
    bulletsComputeShader.SetFloat ("DeltaTime", Time.deltaTime);
    bulletsComputeShader.Dispatch (0, bulletsBuffer.count / 128 + 1, 1, 1);
  }

  void InitializeComputeBuffer () {
    bulletsBuffer = new ComputeBuffer (this.BULLET_MAX, Marshal.SizeOf (typeof (Bullet)));

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

  int FindoUnusedBulletIndex () {
    int index = this.lastBulletIndex;
    int result = -1;
    while (result == -1) {
      if (this.bullets[index].state == 0) {
        result = index;
        this.lastBulletIndex = (index + 1) % this.BULLET_MAX;
      }
      index = (index + 1) % this.BULLET_MAX;
      if (index == this.lastBulletIndex) {
        break;
      }
    }
    return result;
  }

  void createBullet (Bullet bullet) {
    int index = this.FindoUnusedBulletIndex ();
    if (index == -1) {
      // Debug.Log ("bullet max");
      return;
    }
    this.bullets[index] = bullet;
  }
}
