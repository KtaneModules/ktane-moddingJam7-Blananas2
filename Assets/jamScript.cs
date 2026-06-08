using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class jamScript : MonoBehaviour {

    public KMBombModule Module;
    public KMAudio Audio;

    public KMSelectable Button;
    public MeshRenderer[] leftDisplaySegments;
    public MeshRenderer[] rightDisplaySegments;
    public Material[] segmentStateMaterials;
    public GameObject[] quadrantObjects;
    public Transform[] quadrantPivots;

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
    public GameObject vectorscopeLine;
    public SpriteRenderer vectorscopeSlot;
    public Sprite[] vectorscopeSprites;

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
    int currentNumber = 100;
    bool submitAllowed = false;
    Constraint[] currentConstraints = { null, null, null, null };
    Constraint[] previousConstraints = { null, null, null, null };
    string[] currentLogging = { null, null, null, null };
    string[] previousLogging = { null, null, null, null };

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake () 
    {
        moduleId = moduleIdCounter++;

        Button.OnInteract += delegate () { ButtonPress(); return false; };

        Module.OnActivate += delegate () { StartCoroutine(ModuleStart()); };
    }

    // Use this for initialization
    void Start () 
    {
        GeneratePuzzle();
        ObjectReset(currentConstraints);
    }

    void GeneratePuzzle()
    {
        if (!currentConstraints.Contains(null))
            for (int k = 0; k < 4; k++)
            {
                previousConstraints[k] = currentConstraints[k];
                previousLogging[k] = currentLogging[k];
            }
        
        remakeConstraints:
        int[] ObjectTypes = Enumerable.Range(0, 10).ToArray().Shuffle();
        currentConstraints[0] = new Constraint((ConstraintType)ObjectTypes[0], ConstraintPosition.TopLeft);
        currentConstraints[1] = new Constraint((ConstraintType)ObjectTypes[1], ConstraintPosition.TopRight);
        currentConstraints[2] = new Constraint((ConstraintType)ObjectTypes[2], ConstraintPosition.BottomLeft);
        currentConstraints[3] = new Constraint((ConstraintType)ObjectTypes[3], ConstraintPosition.BottomRight);
        for (int i = 0; i < 100; i++)
            if (SolutionValid(currentConstraints, i))
                return;
        goto remakeConstraints;
    }

    void ButtonPress() 
    {
        Button.AddInteractionPunch(1f);

        if (moduleSolved || !submitAllowed)
            return;

        if (previousConstraints.Contains(null))
        {
            Debug.LogFormat("[Galatic Fragility #{0}] Attempted to submit too early. Strike!", moduleId);
            Module.HandleStrike();
            return;
        }
        
        Debug.LogFormat("[Galatic Fragility #{0}] Button pressed at {1}. The objects were the following:", moduleId, currentNumber.ToString().PadLeft(2, '0'));
        for (int l = 0; l < 4; l++)
            Debug.LogFormat("[Galatic Fragility #{0}] {1}", moduleId, previousLogging[l]);

        if (SolutionValid(previousConstraints, currentNumber))
        {
            StopAllCoroutines();
            SetSegmentsToNumber(-1);
            StartCoroutine(SolveAnim());
        } else
        {
            Debug.LogFormat("[Galatic Fragility #{0}] Your submission is invalid. Strike!", moduleId);
            Module.HandleStrike();
        }
    }

    bool SolutionValid(Constraint[] constraints, int number)
    {
        for (int u = 0; u < 4; u++)
            if (constraints[u].NumberPassesConstraint(number) != true)
                return false;
        
        return true;
    }

    IEnumerator ModuleStart()
    {
        StartCoroutine(MoveFour(true));
        SetSegmentsToNumber(-1);
        yield return new WaitForSeconds(0.25f);
        StartCoroutine(Timer());
        yield return null;
    }

    void SetSegmentsToNumber(int number)
    {
        if (number > -1)
        {
            for (int i = 0; i < 7; i++)
            {
                leftDisplaySegments[i].material = segmentStateMaterials[digitSegmentStates[number / 10][i] ? 1 : 0];
                rightDisplaySegments[i].material = segmentStateMaterials[digitSegmentStates[number % 10][i] ? 1 : 0];
            }
        } 
        else
        {
            for (int i = 0; i < 7; i++)
            {
                leftDisplaySegments[i].material = segmentStateMaterials[0];
                rightDisplaySegments[i].material = segmentStateMaterials[0];
            }
        }
    }

    IEnumerator Timer()
    {
        while (!moduleSolved)
        {
            yield return new WaitForSeconds(0.75f);
            submitAllowed = true;
            currentNumber--;
            if (currentNumber < 0)
            {
                submitAllowed = false;
                SetSegmentsToNumber(-1);
                StartCoroutine(MoveFour(false));
                yield return new WaitForSeconds(1f);
                GeneratePuzzle();
                ObjectReset(currentConstraints);
                StartCoroutine(MoveFour(true));
                yield return new WaitForSeconds(1f);
                currentNumber = 99;
            }
            SetSegmentsToNumber(currentNumber);
        }
    }
    
    IEnumerator MoveFour(bool b) {
        int[] Order = Enumerable.Range(0, 4).ToArray().Shuffle();
        for (int q = 0; q < 4; q++)
        {
            StartCoroutine(QuadMove(b, currentConstraints[Order[q]]));
            yield return new WaitForSeconds(0.25f);
        }
    }

    IEnumerator QuadMove(bool d, Constraint cons)
    {
        int qix = (int)cons.constraintPosition;
        int qo = (int)cons.constraintType;
        var quadDoor = quadrantObjects[qix];
        var quadObj = objectWholes[qo];
        var pivotObj = quadrantPivots[qix];
        var startRotation = Quaternion.Euler(0f, 0f, d ? -180f : 0f);
        var endRotation = Quaternion.Euler(0f, 0f, d ? 0f : -180f);

        float elapsed = 0f;
        float duration = 0.25f;

        PutObjectAtPivot(quadObj.transform, pivotObj.transform);
        quadObj.SetActive(true);

        while (elapsed < duration)
        {
            quadDoor.transform.localRotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
            PutObjectAtPivot(quadObj.transform, pivotObj.transform);
            yield return null;
            elapsed += Time.deltaTime;
        }
        quadDoor.transform.localRotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
        PutObjectAtPivot(quadObj.transform, pivotObj.transform);
    }

    void PutObjectAtPivot(Transform o, Transform p)
    {
        o.position = p.position;
        o.rotation = p.rotation;
    }

    void ObjectReset(Constraint[] cons) //only feed into this function when the puzzle is considered valid
    {
        for (int j = 0; j < objectWholes.Length; j++)
            objectWholes[j].SetActive(false);
        for (int k = 0; k < 4; k++)
            SetObject(cons[k]);
    }

    IEnumerator SolveAnim()
    {
        StartCoroutine(MoveFour(false));
        for (int g = 0; g < 10; g++)
        {
            for (int i = 0; i < 7; i++)
            {
                leftDisplaySegments[i].material = segmentStateMaterials[Rnd.Range(0, 2)];
                rightDisplaySegments[i].material = segmentStateMaterials[Rnd.Range(0, 2)];
            }
            yield return new WaitForSeconds(0.1f);   
        }
        for (int i = 0; i < 7; i++)
        {
            leftDisplaySegments[i].material = segmentStateMaterials[(i == 2 || i == 3) ? 0 : 1];
            rightDisplaySegments[i].material = segmentStateMaterials[(i == 2 || i == 3) ? 0 : 1];
        }
        Debug.LogFormat("[Galatic Fragility #{0}] Your submission is valid. Module solved.", moduleId);
        Module.HandlePass();
        moduleSolved = true;
    }

    void SetObject(Constraint cs)
    {
        var data = cs.constraintData;
        GameObject objWhole = objectWholes[0];

        currentLogging[(int)cs.constraintPosition] = cs.ToString();

        /*
        for (int d = 0; d < data.Length; d++)
            Debug.LogFormat("{0}: {1}", d, data[d]);
        Debug.Log(cs.ToString());
        */

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
                objWhole.transform.localRotation = Quaternion.Euler(0f, Rnd.Range(0, 3600) * 0.1f, 0f);
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
                objWhole.transform.localRotation = Quaternion.Euler(0f, Rnd.Range(0, 3600) * 0.1f, 0f);
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
                objWhole.transform.localRotation = Quaternion.Euler(0f, Rnd.Range(-200, 201) * 0.1f, 0f);
                break;
            case ConstraintType.Thermo:
                objWhole = objectWholes[8];
                thermoVisual.mesh = thermoHeights[data[0]];
                thermoLetter.text = (char)('A' + data[1]) + "°";
                break;
            case ConstraintType.Vectorscope:
                objWhole = objectWholes[9];
                vectorscopeSlot.sprite = vectorscopeSprites[data[0]];
                vectorscopeLine.transform.localRotation = Quaternion.Euler(0f, 90f, Rnd.Range(-70, 71) * 0.1f);
                break;
        }
    }

    IEnumerator GearSpin(float dir)
    {
        while (!submitAllowed) { yield return new WaitForSeconds(0.25f); } //wait until the submission period comes
        while (submitAllowed)
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

        SetAllLights(lights);

        while (!submitAllowed) { yield return new WaitForSeconds(0.25f); } //wait until submission period comes
        while (submitAllowed)
        {
            lights.Shuffle();
            SetAllLights(lights);
            yield return new WaitForSeconds(wfs);
        }
    }

    void SetAllLights(List<bool> l)
    {
        for (int s = 0; s < 9; s++)
            matrixSquares[s].material = matrixStateMaterials[l[s] ? 1 : 0];
    }
}