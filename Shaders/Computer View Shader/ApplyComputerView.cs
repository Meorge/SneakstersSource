#if false
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNET_Attempt {
public class ApplyComputerView : MonoBehaviour {
	public Material computerMat;
	public Shader computerOverdrawShader;
	public bool replacementShadersDone = false;
	public bool isComputerMode = false;
	public bool useReplacementShader = false;
	public Camera cam;

	public Color environmentColor = Color.white;

	public void Awake() {
		Camera cam = GetComponent<Camera>();

		
	}

	public void Start() {
		
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination) {
		
		if (GameController.gameController.localPlayer == null) 
		{
			Graphics.Blit(source, destination);
			return;
		}
		if (GameController.gameController.GetLocalPlayerType() == PlayerType.MapGuy) {

			if (!replacementShadersDone && useReplacementShader) {
				cam.SetReplacementShader(computerOverdrawShader, "CustomType");
				replacementShadersDone = true;
			}
			Graphics.Blit(source, destination, computerMat);
		} else {
			Graphics.Blit(source, destination);
		}
	}

	void Update() {
		Shader.SetGlobalColor("_CompOverdrawColor", environmentColor);
	}
}
}
#endif