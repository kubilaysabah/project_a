// TozIsigi.cs
// Toz zerrelerinin görünürlüğü (#126). Zerreler kameranın önünde, kafa lambasının hemen içinde uçar; lamba
// yanarken ışığı doğrudan alıp sahnenin en parlak noktaları olurlar; lamba sönükken bile dolgu ışığıyla
// göründükleri için kapkara sahanlıkta "parlayan kar taneleri" gibi okunuyorlardı. Bu bileşen zerrelerin
// alfasını lambaya göre sürer: yanarken tam, sönükken neredeyse yok (0,5 sn yumuşak geçiş).
//
// NASIL: Parçacık malzemesi (Particles/Simple Lit) rengini _BaseColor × tepe rengi olarak kullanır. Malzeme
// paylaşılan bir varlık; onu değiştirmek yerine MaterialPropertyBlock ile yalnızca bu çizicinin _BaseColor'u
// yazılır (AnomaliUygulayici'deki renk kuralı: malzemeyi klonlama). KafaLambasi.Start kamerada
// "Toz_Zerreleri" varsa bu bileşeni ekler; kurucu değişmez.

using UnityEngine;

public class TozIsigi : MonoBehaviour
{
    private const float TamAlfa = 0.42f;    // lamba yanarken
    private const float SonukAlfa = 0.06f;  // lamba sönükken (göz karanlığa alışınca birkaç zerre)
    private const float Hiz = 2f;           // alfa saniyede en çok bu kadar değişir

    private ParticleSystemRenderer cizici;
    private MaterialPropertyBlock blok;
    private KafaLambasi lamba;
    private Color temel;
    private float alfa = TamAlfa;

    private void Start()
    {
        cizici = GetComponent<ParticleSystemRenderer>();
        lamba = GetComponentInParent<KafaLambasi>();
        if (lamba == null) lamba = FindAnyObjectByType<KafaLambasi>();
        blok = new MaterialPropertyBlock();
        temel = cizici != null && cizici.sharedMaterial != null && cizici.sharedMaterial.HasProperty("_BaseColor")
              ? cizici.sharedMaterial.GetColor("_BaseColor") : new Color(1f, 0.97f, 0.9f, 1f);
    }

    private void Update()
    {
        if (cizici == null) return;
        float hedef = lamba != null && lamba.IsikYaniyor ? TamAlfa : SonukAlfa;
        alfa = Mathf.MoveTowards(alfa, hedef, Hiz * Time.deltaTime);
        cizici.GetPropertyBlock(blok);
        blok.SetColor("_BaseColor", new Color(temel.r, temel.g, temel.b, alfa));
        cizici.SetPropertyBlock(blok);
    }
}
