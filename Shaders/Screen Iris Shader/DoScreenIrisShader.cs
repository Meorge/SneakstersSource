using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DoScreenIrisShader : MonoBehaviour {

	public Material irisMaterial;
	public Vector2 irisTarget = new Vector2(0.5f, 0.5f);
	public float distThreshold = 10f;
	public Color outColor = Color.black;

	public float widthRatio = 1f;
	public float hardness = 1f;

	public float speedMult = 1.3f;

	void Start() {
		irisMaterial = new Material(Shader.Find("Hidden/ScreenIrisShader"));
	}
	void OnRenderImage(RenderTexture source, RenderTexture destination) {
		irisMaterial.SetVector("_TargetPos", new Vector4(irisTarget.x, irisTarget.y, 0f, 0f));
		irisMaterial.SetFloat("_Distance", distThreshold);
		irisMaterial.SetFloat("_WidthRatio", widthRatio);
		irisMaterial.SetFloat("_Hardness", hardness);
		irisMaterial.SetColor("_OutColor", outColor);
    	Graphics.Blit(source, destination, irisMaterial);
	}

	public IEnumerator IrisOut() {
		float irisAmt = 1f;
		while (irisAmt > 0f) {
			irisAmt -= Time.deltaTime * speedMult;
			distThreshold = irisAmt;
			yield return new WaitForEndOfFrame();
		}
		yield return null;
	}

	public IEnumerator IrisIn() {
		float irisAmt = 0f;
		while (irisAmt < 1f) {
			irisAmt += Time.deltaTime * speedMult;
			distThreshold = irisAmt;
			yield return new WaitForEndOfFrame();
		}
		yield return null;
	}
}
