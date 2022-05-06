using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VolumeLight
{
    [ExecuteInEditMode]
    public class tkVlit_SpotLight : MonoBehaviour
    {
        VolumeSpotLightData m_data = new VolumeSpotLightData(); // ボリュームスポットライトデータ。
        public VolumeSpotLightData volumeSpotLightData { get { return m_data; } } 
        public int id { private get; set; }     // ボリュームライトのID
        Material[] m_materialRenderVolumeMap;   // ボリュームマップを描画するマテリアル。
        int m_shaderPropertyId_volumeLightID;   // ボリュームライトIDのシェーダーID
                                                // Start is called before the first frame update
        [ColorUsage(false, true), SerializeField] private Color color1 = Color.white;
        [ColorUsage(false, true), SerializeField] private Color color2;
        [ColorUsage(false, true), SerializeField] private Color color3;
        [SerializeField, Range(0f, 160f)] float angle1 = 45.0f;
        [SerializeField, Range(0f, 160f)] float angle2 = 45.0f;
        [SerializeField, Range(0f, 160f)] float angle3 = 45.0f;
        [SerializeField, Range(0f, 10.0f)] float anglePow1 = 1.0f;
        [SerializeField, Range(0f, 10.0f)] float anglePow2 = 1.0f;
        [SerializeField, Range(0f, 10.0f)] float anglePow3 = 1.0f;
        [SerializeField] float range1 = 0.0f;
        [SerializeField] float range2 = 0.0f;
        [SerializeField] float range3 = 0.0f;
        [SerializeField, Range(0f, 10.0f)] float rangePow1 = 1.0f;
        [SerializeField, Range(0f, 10.0f)] float rangePow2 = 1.0f;
        [SerializeField, Range(0f, 10.0f)] float rangePow3 = 1.0f;

        private void OnValidate()
        {
            const float rangeMin = 0.0001f;
            range1 = Mathf.Max(range1, rangeMin);
            range2 = Mathf.Max(range2, rangeMin);
            range3 = Mathf.Max(range3, rangeMin);

            const float powMin = 0.0001f;
            anglePow1 = Mathf.Max(anglePow1, powMin);
            anglePow2 = Mathf.Max(anglePow2, powMin);
            anglePow3 = Mathf.Max(anglePow3, powMin);

            rangePow1 = Mathf.Max(rangePow1, powMin);
            rangePow2 = Mathf.Max(rangePow2, powMin);
            rangePow3 = Mathf.Max(rangePow3, powMin);
        }

        void Start()
        {
            tkVlit_DrawVolumeLight.instance.AddSpotLight(this);

            var meshRendererArray = transform.GetComponentsInChildren<MeshRenderer>();
            m_materialRenderVolumeMap = new Material[meshRendererArray.Length];
            for (int i = 0; i < meshRendererArray.Length; i++)
            {
                m_materialRenderVolumeMap[i] = meshRendererArray[i].sharedMaterial;
            }
            m_shaderPropertyId_volumeLightID = Shader.PropertyToID("volumeLightID");
        }
        private void OnDestroy()
        {
            if (tkVlit_DrawVolumeLight.instance != null)
            {
                tkVlit_DrawVolumeLight.instance.RemoveSpotLight(this);
            }
        }
        // Update is called once per frame
        void Update()
        {
            // IDを割り当てる。
            //AssignIDToVolueLight.instance.AssignIDToVolumeLight(this);
            //foreach (var material in m_materialRenderVolumeMap)
            //{
            //    material.SetInt(m_shaderPropertyId_volumeLightID, id);
            //}

            m_data.no = id;
            m_data.position = transform.position;
            m_data.direction = transform.rotation * Vector3.forward;
            m_data.color.x = color1.r;
            m_data.color.y = color1.g;
            m_data.color.z = color1.b;

            m_data.color2.x = color2.r;
            m_data.color2.y = color2.g;
            m_data.color2.z = color2.b;

            m_data.color3.x = color3.r;
            m_data.color3.y = color3.g;
            m_data.color3.z = color3.b;

            m_data.halfAngleRad.x = Mathf.Deg2Rad * angle1 * 0.5f;
            m_data.halfAngleRad.y = Mathf.Deg2Rad * angle2 * 0.5f;
            m_data.halfAngleRad.z = Mathf.Deg2Rad * angle3 * 0.5f;

            m_data.anglePow.x = anglePow1;
            m_data.anglePow.y = anglePow2;
            m_data.anglePow.z = anglePow3;

            m_data.range.x = range1;
            m_data.range.y = range2;
            m_data.range.z = range3;

            m_data.rangePow.x = rangePow1;
            m_data.rangePow.y = rangePow2;
            m_data.rangePow.z = rangePow3;

            // スポットライトの射出距離と射出角度からスポットライトコーンモデルの拡大率を計算する。
            float maxRange = Mathf.Max( range1, range2, range3 );
            float maxHalfAngle = Mathf.Max( m_data.halfAngleRad.x, m_data.halfAngleRad.y, m_data.halfAngleRad.z ) ;
            // スポットライトで射出角度(ハーフ)が90度を超えてはだめなので、制限をかける。
            maxHalfAngle = Mathf.Min(Mathf.PI * 0.49f, maxHalfAngle);
            float xyScale = Mathf.Tan(maxHalfAngle) * maxRange;

            Vector3 scale;
            scale.x = xyScale;
            scale.y = xyScale;
            scale.z = maxRange;
            transform.localScale = scale;
          //  CameraForRenderVolumeSpotLightFinal.instance.RegisterVolumeSpotLightData(m_data);
        }
    }
}
