using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class jamScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public MeshRenderer[] leftDisplaySegments;
    public MeshRenderer[] rightDisplaySegments;
    public Material[] segmentStateMaterials;
    public GameObject[] coverObjects;

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
    int currentNumber = 99;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake () {
        moduleId = moduleIdCounter++;
        /*
        foreach (KMSelectable object in keypad) {
            object.OnInteract += delegate () { keypadPress(object); return false; };
        }
        */

        //button.OnInteract += delegate () { buttonPress(); return false; };

    }

    // Use this for initialization
    void Start () {
        SetDisplayToNumber(99);
        StartCoroutine(Timer());
    }

    // Update is called once per frame
    void Update () {

    }

    /*
    void keypadPress(KMSelectable object) {
        
    }
    */

    /*
    void buttonPress() {

    }
    */

    void SetDisplayToNumber(int number)
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
                currentNumber = 99;
            SetDisplayToNumber(currentNumber);

            //temporary
            if (currentNumber < 96 && currentNumber > 91)
                StartCoroutine(Uncover(95 - currentNumber));
        }
    }

    IEnumerator Uncover(int coverIx)
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
}