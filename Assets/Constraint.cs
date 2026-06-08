using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/*
 * The types of constraints the module can generate
 */
public enum ConstraintType
{
    Compass,
    Domino,
    Gear,
    Matrix,
    PCB,
    Screws,
    StampMarkings,
    Sticker,
    Thermo,
    Vectorscope
}

/*
 * The position names of the spaces between timer segments
 */
public enum ConstraintPosition
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

/*
 * The object for a constraint
 * Consists of the type of constraint, position of the constraint and data of the constraint
 */
public class Constraint
{
    public ConstraintType constraintType;
    public ConstraintPosition constraintPosition;
    public int[] constraintData;

    // Sets the variables of the constraint when a new constraint object is created
    public Constraint(ConstraintType type, ConstraintPosition position)
    {
        constraintType = type;
        constraintPosition = position;
        switch (constraintType)
        {
            case ConstraintType.Compass:
                constraintData = new int[] { Random.Range(0, 2), Random.Range(0, 4) }; // Needle color (Red = 0, Black = 1) | Direction of needle (North = 0, East = 1, South = 2, West = 3)
                break;
            case ConstraintType.Domino:
                int[] dominoConstraintData = new int[] { Random.Range(0, 2), Random.Range(0, 10), Random.Range(0, 10) }; // Pip color (White = 0, Black = 1) | Number of top pips | Number of bottom pips
                while (dominoConstraintData[0] == 1 && Math.Abs(dominoConstraintData[1] - dominoConstraintData[2]) < 2) // Make sure black pip color domino has pip count with absolute difference of at least 2
                {
                    dominoConstraintData[1] = Random.Range(0, 10);
                    dominoConstraintData[2] = Random.Range(0, 10);
                }
                constraintData = dominoConstraintData;
                break;
            case ConstraintType.Gear:
                constraintData = new int[] { Random.Range(0, 3) }; // Gear spin direction (Clockwise = 0, Counter-clockwise = 1, Stationary = 2)
                break;
            case ConstraintType.Matrix:
                constraintData = new int[] { Random.Range(0, 10) }; // Number of lit squares
                break;
            case ConstraintType.PCB:
                constraintData = new int[] { Random.Range(0, 2), Random.Range(0, 11) }; // Chip relative to board (Square = 0, 45 degrees = 1) | Trace layout (0 = │, 1 = ─, 2 = ┌, 3 = ┐, 4 = └, 5 = ┘, 6 = ├, 7 = ┤, 8 = ┬, 9 = ┴, 10 = ┼)
                break;
            case ConstraintType.Screws:
                constraintData = new int[] { Random.Range(0, 2), Random.Range(2, 10) }; // Screw type (Phillips = 0, Torx = 1) | Number of screws
                break;
            case ConstraintType.StampMarkings:
                constraintData = new int[] { Random.Range(0, 3) }; // Present markings (CLASSIFIED = 0, DECLASSIFIED = 1, CLASSIFIED & DECLASSIFIED = 2)
                break;
            case ConstraintType.Sticker:
                constraintData = new int[] { Random.Range(0, 6) }; // Letter on sticker (B = 0, D = 1, E = 2, F = 3, G = 4, H = 5)
                break;
            case ConstraintType.Thermo:
                constraintData = new int[] { Random.Range(0, 3), Random.Range(0, 7) }; // Thermometer reading | Letter next to thermo (A = 0, B = 1, ..., G = 6)
                break;
            case ConstraintType.Vectorscope:
                constraintData = new int[] { Random.Range(0, 10) }; // Vectorscope reading
                break;
        }
    }

    // Evaluates if a number passes the constraint or not
    // Returns null if the constraint type, data or relevent segment indexes are an unexpected value
    public bool? NumberPassesConstraint(int number)
    {
        int leftDigit = number / 10;
        int rightDigit = number % 10;
        switch (constraintType)
        {
            case ConstraintType.Compass:
                int[] compassSegmentIndexes = GetCompassSegmentIndexes();
                if (compassSegmentIndexes == null)
                    return null;
                bool[] compassSegmentsLit = new bool[compassSegmentIndexes.Length];
                for (int i = 0; i < compassSegmentsLit.Length; i++)
                    if ((compassSegmentIndexes[i] <= 6 && jamScript.digitSegmentStates[leftDigit][compassSegmentIndexes[i]]) || (compassSegmentIndexes[i] > 6 && jamScript.digitSegmentStates[rightDigit][compassSegmentIndexes[i] - 7]))
                        compassSegmentsLit[i] = true;
                if (constraintData[0] == 0)
                    return compassSegmentsLit.All(x => x);
                else if (constraintData[0] == 1)
                    return compassSegmentsLit.All(x => !x);
                else
                    return null;
            case ConstraintType.Domino:
                if (constraintData.Length != 3 || constraintData[1] < 0 || constraintData[1] > 9 || constraintData[2] < 0 || constraintData[2] > 9 || (constraintData[0] == 1 && Math.Abs(constraintData[1] - constraintData[2]) < 2))
                    return null;
                int absoluteDifference = Math.Abs(leftDigit - rightDigit);
                int minPips = Math.Min(constraintData[1], constraintData[2]);
                int maxPips = Math.Max(constraintData[1], constraintData[2]);
                if (constraintData[0] == 0)
                    return minPips <= absoluteDifference && absoluteDifference <= maxPips;
                else if (constraintData[0] == 1)
                    return minPips < absoluteDifference && absoluteDifference < maxPips;
                else
                    return null;
            case ConstraintType.Gear:
                if (constraintData.Length != 1)
                    return null;
                if (constraintData[0] == 0)
                    return rightDigit > leftDigit;
                else if (constraintData[0] == 1)
                    return leftDigit > rightDigit;
                else if (constraintData[0] == 2)
                    return leftDigit == rightDigit;
                else
                    return null;
            case ConstraintType.Matrix:
                if (constraintData.Length != 1 || constraintData[0] < 0 || constraintData[0] > 9)
                    return null;
                if (constraintPosition == ConstraintPosition.TopLeft)
                    return constraintData[0] >= leftDigit;
                else if (constraintPosition == ConstraintPosition.TopRight)
                    return constraintData[0] >= rightDigit;
                else if (constraintPosition == ConstraintPosition.BottomLeft)
                    return constraintData[0] <= leftDigit;
                else if (constraintPosition == ConstraintPosition.BottomRight)
                    return constraintData[0] <= rightDigit;
                else
                    return null;
            case ConstraintType.PCB:
                int[] pcbSegmentIndexes = GetPCBSegmentIndexes();
                if (pcbSegmentIndexes == null)
                    return null;
                bool[] pcbSegmentsLit = new bool[pcbSegmentIndexes.Length];
                for (int i = 0; i < pcbSegmentsLit.Length; i++)
                    if ((pcbSegmentIndexes[i] <= 6 && jamScript.digitSegmentStates[leftDigit][pcbSegmentIndexes[i]]) || (pcbSegmentIndexes[i] > 6 && jamScript.digitSegmentStates[rightDigit][pcbSegmentIndexes[i] - 7]))
                        pcbSegmentsLit[i] = true;
                if (constraintData[0] == 0)
                    return pcbSegmentsLit.All(x => x) || pcbSegmentsLit.All(x => !x);
                else if (constraintData[0] == 1)
                    return pcbSegmentsLit.Count(x => x) == 1;
                else
                    return null;
            case ConstraintType.Screws:
                if (constraintData.Length != 2 || constraintData[1] < 2 || constraintData[1] > 9)
                    return null;
                if (constraintData[0] == 0)
                    return number % constraintData[1] == 0;
                else if (constraintData[0] == 1)
                    return ((rightDigit * 10) + leftDigit) % constraintData[1] == 0;
                else
                    return null;
            case ConstraintType.StampMarkings:
                if (constraintData.Length != 1)
                    return null;
                if (constraintData[0] == 0)
                    return IsPrime(jamScript.digitSegmentStates[leftDigit].Count(x => x) + jamScript.digitSegmentStates[rightDigit].Count(x => x)) && !IsPrime(number);
                else if (constraintData[0] == 1)
                    return !IsPrime(jamScript.digitSegmentStates[leftDigit].Count(x => x) + jamScript.digitSegmentStates[rightDigit].Count(x => x)) && IsPrime(number);
                else if (constraintData[0] == 2)
                    return IsPrime(jamScript.digitSegmentStates[leftDigit].Count(x => x) + jamScript.digitSegmentStates[rightDigit].Count(x => x)) && IsPrime(number);
                else
                    return null;
            case ConstraintType.Sticker:
                int[][] stickerSegmentIndexes = GetStickerSegmentIndexes();
                if (stickerSegmentIndexes == null)
                    return null;
                for (int i = 0; i < 4; i++)
                {
                    bool[] stickerSegmentsLit = new bool[stickerSegmentIndexes[i].Length];
                    for (int j = 0; j < stickerSegmentsLit.Length; j++)
                        if ((stickerSegmentIndexes[i][j] <= 6 && jamScript.digitSegmentStates[leftDigit][stickerSegmentIndexes[i][j]]) || (stickerSegmentIndexes[i][j] > 6 && jamScript.digitSegmentStates[rightDigit][stickerSegmentIndexes[i][j] - 7]))
                            stickerSegmentsLit[j] = true;
                    if (stickerSegmentsLit.All(x => x))
                        return true;
                }
                return false;
            case ConstraintType.Thermo:
                if (constraintData.Length != 2 || constraintData[0] < 0 || constraintData[0] > 2 || constraintData[1] < 0 || constraintData[1] > 6)
                    return null;
                bool[] segmentsLit = new bool[] { jamScript.digitSegmentStates[leftDigit][constraintData[1]], jamScript.digitSegmentStates[rightDigit][constraintData[1]] };
                return segmentsLit.Count(x => x) == constraintData[0];
            case ConstraintType.Vectorscope:
                if (constraintData.Length != 1 || constraintData[0] < 0 || constraintData[0] > 9)
                    return null;
                return leftDigit != constraintData[0] && rightDigit != constraintData[0];
            default:
                return null;
        }
    }

    // Gets the reading order indexes of each relevent segment for the compass's direction and position
    // Indexes greater than 6 refer to the segments of the right digit
    // Returns null if the constraint type, data or position is an unexpected value
    int[] GetCompassSegmentIndexes()
    {
        if (constraintType != ConstraintType.Compass || constraintData.Length != 2)
            return null;
        switch (constraintPosition)
        {
            case ConstraintPosition.TopLeft:
                if (constraintData[1] == 0)
                    return new int[] { 0 };
                else if (constraintData[1] == 1)
                    return new int[] { 2, 8, 9 };
                else if (constraintData[1] == 2)
                    return new int[] { 3, 6 };
                else if (constraintData[1] == 3)
                    return new int[] { 1 };
                else
                    return null;
            case ConstraintPosition.TopRight:
                if (constraintData[1] == 0)
                    return new int[] { 7 };
                else if (constraintData[1] == 1)
                    return new int[] { 9 };
                else if (constraintData[1] == 2)
                    return new int[] { 10, 13 };
                else if (constraintData[1] == 3)
                    return new int[] { 1, 2, 8 };
                else
                    return null;
            case ConstraintPosition.BottomLeft:
                if (constraintData[1] == 0)
                    return new int[] { 0, 3 };
                else if (constraintData[1] == 1)
                    return new int[] { 5, 11, 12 };
                else if (constraintData[1] == 2)
                    return new int[] { 6 };
                else if (constraintData[1] == 3)
                    return new int[] { 4 };
                else
                    return null;
            case ConstraintPosition.BottomRight:
                if (constraintData[1] == 0)
                    return new int[] { 7, 10 };
                else if (constraintData[1] == 1)
                    return new int[] { 12 };
                else if (constraintData[1] == 2)
                    return new int[] { 13 };
                else if (constraintData[1] == 3)
                    return new int[] { 4, 5, 11 };
                else
                    return null;
            default:
                return null;
        }
    }

    // Gets the reading order indexes of each relevent segment for the PCB's trace and position
    // Indexes greater than 6 refer to the segments of the right digit
    // Returns null if the constraint type, data or position is an unexpected value
    int[] GetPCBSegmentIndexes()
    {
        if (constraintType != ConstraintType.PCB || constraintData.Length != 2)
            return null;
        switch (constraintPosition)
        {
            case ConstraintPosition.TopLeft:
                if (constraintData[1] == 0)
                    return new int[] { 0, 3 };
                else if (constraintData[1] == 1)
                    return new int[] { 1, 2 };
                else if (constraintData[1] == 2)
                    return new int[] { 2, 3 };
                else if (constraintData[1] == 3)
                    return new int[] { 1, 3 };
                else if (constraintData[1] == 4)
                    return new int[] { 0, 2 };
                else if (constraintData[1] == 5)
                    return new int[] { 0, 1 };
                else if (constraintData[1] == 6)
                    return new int[] { 0, 2, 3 };
                else if (constraintData[1] == 7)
                    return new int[] { 0, 1, 3 };
                else if (constraintData[1] == 8)
                    return new int[] { 1, 2, 3 };
                else if (constraintData[1] == 9)
                    return new int[] { 0, 1, 2 };
                else if (constraintData[1] == 10)
                    return new int[] { 0, 1, 2, 3 };
                else
                    return null;
            case ConstraintPosition.TopRight:
                if (constraintData[1] == 0)
                    return new int[] { 7, 10 };
                else if (constraintData[1] == 1)
                    return new int[] { 8, 9 };
                else if (constraintData[1] == 2)
                    return new int[] { 9, 10 };
                else if (constraintData[1] == 3)
                    return new int[] { 8, 10 };
                else if (constraintData[1] == 4)
                    return new int[] { 7, 9 };
                else if (constraintData[1] == 5)
                    return new int[] { 7, 8 };
                else if (constraintData[1] == 6)
                    return new int[] { 7, 9, 10 };
                else if (constraintData[1] == 7)
                    return new int[] { 7, 8, 10 };
                else if (constraintData[1] == 8)
                    return new int[] { 8, 9, 10 };
                else if (constraintData[1] == 9)
                    return new int[] { 7, 8, 9 };
                else if (constraintData[1] == 10)
                    return new int[] { 7, 8, 9, 10 };
                else
                    return null;
            case ConstraintPosition.BottomLeft:
                if (constraintData[1] == 0)
                    return new int[] { 3, 6 };
                else if (constraintData[1] == 1)
                    return new int[] { 4, 5 };
                else if (constraintData[1] == 2)
                    return new int[] { 5, 6 };
                else if (constraintData[1] == 3)
                    return new int[] { 4, 6 };
                else if (constraintData[1] == 4)
                    return new int[] { 3, 5 };
                else if (constraintData[1] == 5)
                    return new int[] { 3, 4 };
                else if (constraintData[1] == 6)
                    return new int[] { 3, 5, 6 };
                else if (constraintData[1] == 7)
                    return new int[] { 3, 4, 6 };
                else if (constraintData[1] == 8)
                    return new int[] { 4, 5, 6 };
                else if (constraintData[1] == 9)
                    return new int[] { 3, 4, 5 };
                else if (constraintData[1] == 10)
                    return new int[] { 3, 4, 5, 6 };
                else
                    return null;
            case ConstraintPosition.BottomRight:
                if (constraintData[1] == 0)
                    return new int[] { 10, 13 };
                else if (constraintData[1] == 1)
                    return new int[] { 11, 12 };
                else if (constraintData[1] == 2)
                    return new int[] { 12, 13 };
                else if (constraintData[1] == 3)
                    return new int[] { 11, 13 };
                else if (constraintData[1] == 4)
                    return new int[] { 10, 12 };
                else if (constraintData[1] == 5)
                    return new int[] { 10, 11 };
                else if (constraintData[1] == 6)
                    return new int[] { 10, 12, 13 };
                else if (constraintData[1] == 7)
                    return new int[] { 10, 11, 13 };
                else if (constraintData[1] == 8)
                    return new int[] { 11, 12, 13 };
                else if (constraintData[1] == 9)
                    return new int[] { 10, 11, 12 };
                else if (constraintData[1] == 10)
                    return new int[] { 10, 11, 12, 13 };
                else
                    return null;
            default:
                return null;
        }
    }

    // Gets the reading order indexes of each relevent segment for the Sticker's letter for each position
    // Indexes greater than 6 refer to the segments of the right digit
    // Returns null if the constraint type or data is an unexpected value
    int[][] GetStickerSegmentIndexes()
    {
        if (constraintType != ConstraintType.Sticker || constraintData.Length != 1)
            return null;
        if (constraintData[0] == 0)
            return new int[][] { new int[] { 1, 2, 3 }, new int[] { 8, 9, 10 }, new int[] { 4, 5, 6 }, new int[] { 11, 12, 13 } };
        else if (constraintData[0] == 1)
            return new int[][] { new int[] { 0, 2, 3 }, new int[] { 7, 9, 10 }, new int[] { 3, 5, 6 }, new int[] { 10, 12, 13 } };
        else if (constraintData[0] == 2)
            return new int[][] { new int[] { 0, 1, 2, 3 }, new int[] { 7, 8, 9, 10 }, new int[] { 3, 4, 5, 6 }, new int[] { 10, 11, 12, 13 } };
        else if (constraintData[0] == 3)
            return new int[][] { new int[] { 0, 1, 3 }, new int[] { 7, 8, 10 }, new int[] { 3, 4, 6 }, new int[] { 10, 11, 13 } };
        else if (constraintData[0] == 4)
            return new int[][] { new int[] { 0, 2 }, new int[] { 7, 9 }, new int[] { 3, 5 }, new int[] { 10, 12 } };
        else if (constraintData[0] == 5)
            return new int[][] { new int[] { 0, 1, 2 }, new int[] { 7, 8, 9 }, new int[] { 3, 4, 5 }, new int[] { 10, 11, 12 } };
        else
            return null;
    }

    // Returns true if the provided number is prime
    bool IsPrime(int number)
    {
        if (number <= 1) return false;
        if (number == 2) return true;
        if (number % 2 == 0) return false;

        var boundary = (int)Math.Floor(Math.Sqrt(number));

        for (int i = 3; i <= boundary; i += 2)
            if (number % i == 0)
                return false;

        return true;
    }

    // Overrides the ToString method to return a custom string for the constraint
    public override string ToString()
    {
        string constraintDataString = "";
        switch (constraintType)
        {
            case ConstraintType.Compass:
                string[] compassColors = { "red", "black" };
                string[] compassDirections = { "north", "east", "south", "west" };
                constraintDataString = "with a " + compassColors[constraintData[0]] + " needle pointing " + compassDirections[constraintData[1]];
                break;
            case ConstraintType.Domino:
                string[] dominoColors = { "white", "black" };
                constraintDataString = "with " + dominoColors[constraintData[0]] + " pips that reads " + constraintData[1] + "|" + constraintData[2];
                break;
            case ConstraintType.Gear:
                string[] gearDirections = { "spinning clockwise", "spinning counter-clockwise", "that is stationary" };
                constraintDataString = gearDirections[constraintData[0]];
                break;
            case ConstraintType.Matrix:
                constraintDataString = "with " + constraintData[0] + " lit squares";
                break;
            case ConstraintType.PCB:
                string[] pcbAngles = { "square", "45 degrees" };
                string[] pcbTraces = { "│", "─", "┌", "┐", "└", "┘", "├", "┤", "┬", "┴", "┼" };
                constraintDataString = "with a chip that is " + pcbAngles[constraintData[0]] + " relative to the board and a " + pcbTraces[constraintData[1]] + " trace";
                break;
            case ConstraintType.Screws:
                string[] screwTypes = { "Phillips", "Torx" };
                constraintDataString = "with " + constraintData[1] + " total of type " + screwTypes[constraintData[0]];
                break;
            case ConstraintType.StampMarkings:
                string[] markingsPresent = { "CLASSIFIED", "DECLASSIFIED", "CLASSIFIED & DECLASSIFIED" };
                constraintDataString = "with markings " + markingsPresent[constraintData[0]];
                break;
            case ConstraintType.Sticker:
                string[] stickerLetters = { "B", "D", "E", "F", "G", "H" };
                constraintDataString = "of the letter " + stickerLetters[constraintData[0]];
                break;
            case ConstraintType.Thermo:
                string[] thermoLetters = { "A", "B", "C", "D", "E", "F", "G" };
                constraintDataString = "that reads " + constraintData[0] + "°" + thermoLetters[constraintData[1]];
                break;
            case ConstraintType.Vectorscope:
                constraintDataString = "with a line going through box " + constraintData[0];
                break;
        }
        string[] positionNames = { "top-left", "top-right", "bottom-left", "bottom-right" };
        return string.Format("The {0} constraint was {1} {2}", positionNames[(int)constraintPosition], constraintType, constraintDataString).Replace("StampMarkings", "Stamp Markings");
    }
}
