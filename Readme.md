## AzureField 공개 스크립트/쉐이더

**이 파일들에는 본인이 Project AzureField를 개발하며 작업했던 모든 스크립트와 쉐이더가 있습니다.**

사용한 유니티 에디터 버전 : 2023.3.16f
렌더 파이프라인 : Universal

**폴더 내용**
 - Scripts : C# 스크립트 파일들입니다.
- Shaders : 쉐이더 파일들입니다.
	- Amplify Shader : Amplify 쉐이더 에디터를 사용한 쉐이더 그래프들입니다.
	- Unity Shader Graph : URP 쉐이더 그래프를 사용한 쉐이더 그래프들입니다.


**스크립트 문서**
[Documentation](https://badtoast.notion.site/1239a901e61d80f28f9ee762a82704c4?v=1239a901e61d81a28b28000cc5719bb0&pvs=4)


## 주요 스크립트

- Scripts/StaticSerializedMonoBehavior.cs
	이 클래스를 상속받으면 MonoBehaviour를 사용하는 static인스턴스를 생성할 수 있습니다.

 - 바다 관련 스크립트
	- Scripts/Managers/GlobalOceanManager.cs 
			현재 월드상의 바다와 관련된 데이터를 관리하고 물리적인 연산을 하는 시스템 클래스입니다.
	- Scripts/Managers/OceanProfile.cs
			현재 바다 표면의 정보를 담는 스크립터블 오브젝트입니다.
	- Scripts/Physics/BuoyantBehavior.cs
			Rigidbody 컴포넌트에 바다의 부력을 통한 물리적인 힘을 추가해줄 수 있습니다.

- 시퀀스 관련 스크립트
	-  Scripts/Narratives/SequenceData.cs
	시퀀스번들의 정보를 나타내는 스크립트입니다. 
	게임플레이 중 대사, 이벤트, 타임라인 같은 것들을 코루틴을 이용해 연속적으로 순서대로 재생할 수 있도록 합니다.
	- Scripts/Narratives/SequenceInvoker.cs
		시퀀스 번들을 재생할 수 있는 스크립트 입니다.

- 플레이어 관련 스크립트
	- Scripts/Player/PlayerCore.cs
		플레이어 캐릭터에 관한 핵심 코드들입니다.
	- Scripts/Player/SailboatBehavior.cs
		플레이어가 타는 보드를 제어하는 스크립트입니다.

- 상호작용 관련 스크립트
	-  Scripts/Interactions/Interactable_Base.cs
		Interactable 계열 스크립트들의 베이스 클래스입니다.
		플레이어가 가까이 오면서 Trigger를 건드리면 상호작용 UI를 띄우고, 
		상호작용 키를 누르면 오버라이드된 Interact()를 호출합니다.

- 아이템 정보 관련
	- Scripts/Items/ItemData.cs
		아이템 정보를 담아두는 스크립터블 오브젝트입니다.

- 컨텐츠 : 순풍의 도전 관련
	- Scripts/Levels/FairwindChallengeInstance.cs
	순풍의 도전 단일 개체에 대한 스크립트입니다.
		
- 섬 지역 관리
	- Scripts/Levels/IslandArea.cs
	섬의 고유 정보와 영역을 나타내는 스크립트 입니다.

- 주변 환경 관련
	- Scripts/Managers/AtmosphereManager.cs
	플레이어의 주변 환경을 통제하는 스크립트입니다.
	- Scripts/Misc/AzfAtmosProfile.cs
	주변 환경 데이터를 담는 스크립터블 오브젝트입니다.
	
## 주요 쉐이더

- Amplify 쉐이더
	- BorderBarrier.shader
	방벽 효과 쉐이더입니다.
	- Foliage.shader
	나뭇잎 쉐이더입니다.
	- UI_TextureImageCaustic.shader
	Image(UI)에 Caustic효과를 줍니다.

- UnityShaderGraph
	- AlphamapLit.shadergraph
		Opaque Surface에 디더링을 적용하여 반투명 효과를 모사할 수 있습니다.
	- ShadeRemapLit.shadergraph
		그림자를 Remap하여 색상을 바꿀 수 있습니다. ( 하프램버트 기반 )
	- Ocean/OceanSurface.shadergraph
		바다 표면을 그리는 최종 쉐이더입니다.
