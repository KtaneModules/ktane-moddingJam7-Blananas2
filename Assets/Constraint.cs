using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    Thermo
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
    ConstraintType constraintType;
    ConstraintPosition constraintPosition;
    int[] constraintData;

    // Randomly generate a constraint when a new constraint object is created
    public Constraint(ConstraintPosition position)
    {
        constraintType = (ConstraintType)Random.Range(0, 9);
        constraintPosition = position;
        switch (constraintType)
        {
            case ConstraintType.Compass:
                constraintData = new int[] { Random.Range(0, 2), Random.Range(0, 4) }; // Needle color (Red = 0, Black = 1) | Direction of needle (North = 0, East = 1, South = 2, West = 3)
                break;
            case ConstraintType.Domino:
                constraintData = new int[] { Random.Range(0, 2), Random.Range(0, 10), Random.Range(0, 10) }; // Pip color (White = 0, Black = 1) | Number of top pips | Number of bottom pips
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
                constraintData = new int[] { Random.Range(0, 2), Random.Range(1, 10) }; // Screw type (Phillips = 0, Torx = 1) | Number of screws
                break;
            case ConstraintType.StampMarkings:
                constraintData = new int[] { Random.Range(0, 3) }; // Present markings (CLASSIFIED = 0, DECLASSIFIED = 1, CLASSIFIED & DECLASSIFIED = 2)
                break;
            case ConstraintType.Sticker:
                constraintData = new int[] { Random.Range(0, 18) }; // Letter on sticker (A = 0, B = 1, ..., R = 17)
                break;
            case ConstraintType.Thermo:
                constraintData = new int[] { Random.Range(0, 3), Random.Range(0, 7) }; // Thermometer reading | Letter next to thermo (A = 0, B = 1, ..., G = 6)
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
                int[] releventSegments = GetCompassSegmentIndexes();
                if (releventSegments == null)
                    return null;
                bool[] segmentsLit = new bool[releventSegments.Length];
                for (int i = 0; i < segmentsLit.Length; i++)
                    if ((releventSegments[i] <= 6 && jamScript.digitSegmentStates[leftDigit][releventSegments[i]]) || (releventSegments[i] > 6 && jamScript.digitSegmentStates[rightDigit][releventSegments[i] - 7]))
                        segmentsLit[i] = true;
                if (constraintData[0] == 0)
                    return segmentsLit.All(x => x);
                else if (constraintData[0] == 1)
                    return segmentsLit.All(x => !x);
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
}
