using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class jamScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;

    public MeshRenderer[] leftDisplaySegments;
    public MeshRenderer[] rightDisplaySegments;
    public Material[] segmentStateMaterials;
    public GameObject[] coverObjects;

    public GameObject[] objectWholes; //these contain the entirety of what makes up each object; ordered same as ConstraintType enum
    public MeshRenderer compassNeedle;
    public Material[] compassMaterials;
    public MeshRenderer dominoPlate;
    public Material[] dominoMaterials;
    public SpriteRenderer[] dominoPipSlots;
    public Sprite[] dominoPips; //also includes the bar in the middle at index 2
    public MeshRenderer[] matrixSquares;
    public Material[] matrixStateMaterials;
    public SpriteRenderer pcbTraceSlot;
    public Sprite[] pcbTraceSprites; //ordered same as trace layout comment in Constraint.cs
    public GameObject pcbChip;
    public GameObject[] screwsIndiv;
    public MeshFilter[] screwsVisual;
    public Mesh[] screwTypes;
    public GameObject[] stampsIndiv;
    public SpriteRenderer[] stickerSlots; // Letter = 0, Back = 1
    public Sprite[] stickerLetterSprites;
    public Sprite[] stickerBackSprites;
    public MeshFilter thermoVisual;
    public Mesh[] thermoHeights;
    public TextMesh thermoLetter;

    public static bool[][] digitSegmentStates =
    {
        new bool[] { true, true, true, false, true, true, true },     // 0
        new bool[] { false, false, true, false, false, true, false }, // 1
        new bool[] { true, false, true, true, true, false, true },    // 2
        new bool[] { true, false, true, true, false, true, true },    // 3
        new bool[] { false, true, true, true, false, true, false },   // 4
        new bool[] { true, true, false, true, false, true, true },    // 5
        new bool[] { true, true, false, true, true, true, true },     // 6
        new bool[] { true, false, true, false, false, true, false },  // 7
        new bool[] { true, true, true, true, true, true, true },      // 8
        new bool[] { true, true, true, true, false, true, true }      // 9
    };
    public static Vector3[] quadrantPositions =
    {
        new Vector3(-0.036f, 0.015f, 0.011f),
        new Vector3(0.036f, 0.015f, 0.011f),
        new Vector3(-0.036f, 0.015f, -0.0409f),
        new Vector3(0.036f, 0.015f, -0.0409f)
    };
    int currentNumber = 99;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake () {
        moduleId = moduleIdCounter++;

        //button.OnInteract += delegate () { buttonPress(); return false; };
    }

    // Use this for initialization
    void Start () {
        SetSegmentsToNumber(99);
        Constraint topLeftConstraint = new Constraint(ConstraintType.PCB, ConstraintPosition.TopLeft);
        Constraint topRightConstraint = new Constraint(ConstraintType.Screws, ConstraintPosition.TopRight);
        Constraint bottomLeftConstraint = new Constraint(ConstraintType.StampMarkings, ConstraintPosition.BottomLeft);
        Constraint bottomRightConstraint = new Constraint(ConstraintType.Thermo, ConstraintPosition.BottomRight);
        ObjectReset(topLeftConstraint, topRightConstraint, bottomLeftConstraint, bottomRightConstraint);
    }

    // Update is called once per frame
    void Update () {

    }

    /*
    void buttonPress() {

    }
    */

    void SetSegmentsToNumber(int number)
    {
        for (int i = 0; i < 7; i++)
        {
            leftDisplaySegments[i].material = digitSegmentStates[number / 10][i] ? segmentStateMaterials[1] : segmentStateMaterials[0];
            rightDisplaySegments[i].material = digitSegmentStates[number % 10][i] ? segmentStateMaterials[1] : segmentStateMaterials[0];
        }
    }

    IEnumerator Timer()
    {
        while (!moduleSolved)
        {
            yield return new WaitForSeconds(0.75f);
            currentNumber--;
            if (currentNumber < 0)
                currentNumber = 99; //later, handle switching the puzzle
            SetSegmentsToNumber(currentNumber);

            /* //this here is to test the uncover function
            if (currentNumber < 96 && currentNumber > 91)
                StartCoroutine(Uncover(95 - currentNumber));
            */
        }
    }

    IEnumerator Uncover(int coverIx) //this function currently does not work, rotation doesn't look quite right; may replace entirely
    {
        var coverObj = coverObjects[coverIx];
        var startRotation = Quaternion.Euler(0f, 0f, 0f);
        var endRotation = Quaternion.Euler(-180f, 0f, 0f);

        float elapsed = 0f;
        float duration = 0.25f;

        while (elapsed < duration)
        {
            coverObj.transform.localRotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
            yield return null;
            elapsed += Time.deltaTime;
        }
        coverObj.SetActive(false);
    }

    void ObjectReset(Constraint tlc, Constraint trc, Constraint blc, Constraint brc) //only feed into this function when the puzzle is considered valid
    {
        StopAllCoroutines();
        for (int j = 0; j < objectWholes.Length; j++)
            objectWholes[j].SetActive(false);
        SetObject(tlc);
        SetObject(trc);
        SetObject(blc);
        SetObject(brc);
        StartCoroutine(Timer());
    }

    void SetObject(Constraint cs)
    {
        var data = cs.constraintData;
        GameObject objWhole = objectWholes[0];

        Debug.Log(cs.ToString());
        for (int d = 0; d < data.Length; d++)
            Debug.LogFormat("{0}: {1}", d, data[d]);

        switch (cs.constraintType)
        {
            case ConstraintType.Compass:
                objWhole = objectWholes[0];
                compassNeedle.material = compassMaterials[data[0]];
                objWhole.transform.localRotation = Quaternion.Euler(0f, data[1] * 90f, 0f);
                break;
            case ConstraintType.Domino:
                objWhole = objectWholes[1];
                dominoPlate.material = dominoMaterials[data[0]];
                for (int s = 0; s < 3; s++)
                    dominoPipSlots[s].color = data[0] == 0 ? Color.white : Color.black;
                dominoPipSlots[0].sprite = dominoPips[data[1]];
                dominoPipSlots[1].sprite = dominoPips[data[2]];
                objWhole.transform.localRotation = Quaternion.Euler(0f, Rnd.Range(0, 360) * 1f, 0f);
                break;
            case ConstraintType.Gear:
                objWhole = objectWholes[2];
                if (data[0] != 2)
                    StartCoroutine(GearSpin(data[0] == 0 ? 0.3f : -0.3f));
                break;
            case ConstraintType.Matrix:
                objWhole = objectWholes[3];
                StartCoroutine(MatrixFlash(data[0]));
                break;
            case ConstraintType.PCB:
                objWhole = objectWholes[4];
                pcbChip.transform.localRotation = Quaternion.Euler(0f, data[0] == 0 ? 0f : 45f, 0f);
                pcbTraceSlot.sprite = pcbTraceSprites[data[1]];
                break;
            case ConstraintType.Screws: //this could be improved to look more random but aint nobody got time for that on a deadline (:
                objWhole = objectWholes[5];
                List<bool> ourScrews = new List<bool> { };
                for (int s = 0; s < 9; s++)
                {
                    screwsVisual[s].mesh = screwTypes[data[0]];
                    ourScrews.Add(s < data[1]);
                }
                ourScrews.Shuffle();
                for (int s = 0; s < 9; s++)
                    screwsIndiv[s].SetActive(ourScrews[s]);
                objWhole.transform.localRotation = Quaternion.Euler(0f, Rnd.Range(0, 360) * 1f, 0f);
                objWhole.transform.localScale = new UnityEngine.Vector3(Rnd.Range(0, 2) == 0 ? 0.9f : -0.9f, 0.9f, Rnd.Range(0, 2) == 0 ? 0.9f : -0.9f);
                break;
            case ConstraintType.StampMarkings:
                objWhole = objectWholes[6];
                stampsIndiv[0].SetActive(data[0] != 1);
                stampsIndiv[1].SetActive(data[0] != 0);
                break;
            case ConstraintType.Sticker:
                objWhole = objectWholes[7];
                stickerSlots[0].sprite = stickerLetterSprites[data[0]];
                stickerSlots[1].sprite = stickerBackSprites[Rnd.Range(0, stickerBackSprites.Length)];
                objWhole.transform.localRotation = Quaternion.Euler(0f, Rnd.Range(-20, 21) * 1f, 0f);
                break;
            case ConstraintType.Thermo:
                objWhole = objectWholes[8];
                thermoVisual.mesh = thermoHeights[data[0]];
                thermoLetter.text = (char)('A' + data[1]) + "°";
                break;
        }

        switch (cs.constraintPosition)
        {
            case ConstraintPosition.TopLeft: objWhole.transform.localPosition = quadrantPositions[0]; break;
            case ConstraintPosition.TopRight: objWhole.transform.localPosition = quadrantPositions[1]; break;
            case ConstraintPosition.BottomLeft: objWhole.transform.localPosition = quadrantPositions[2]; break;
            case ConstraintPosition.BottomRight: objWhole.transform.localPosition = quadrantPositions[3]; break;
        }

        objWhole.SetActive(true);
    }

    IEnumerator GearSpin(float dir)
    {
        while (true)
        {
            objectWholes[2].transform.Rotate(new Vector3(0, dir, 0));
            yield return null;
        }
    }

    IEnumerator MatrixFlash(int count)
    {
        List<bool> lights = new List<bool> { };
        for (int l = 0; l < 9; l++)
            lights.Add(l < count);
        
        float wfs = (count == 0 || count == 9) ? 100f : 0.75f;

        while (true)
        {
            lights.Shuffle();
            for (int s = 0; s < 9; s++)
                matrixSquares[s].material = matrixStateMaterials[lights[s] ? 1 : 0];
            yield return new WaitForSeconds(wfs);
        }
    }
}