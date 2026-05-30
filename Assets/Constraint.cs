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
                constraintData = new int[] { Random.Range(0, 9), Random.Range(0, 9) }; // Number of top pips | Number of bottom pips
                break;
            case ConstraintType.Gear:
                constraintData = new int[] { Random.Range(0, 3) }; // Gear spin direction (Clockwise = 0, Counter-clockwise = 1, Stationary = 2)
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
                if (constraintData.Length != 2)
                    return null;
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
            default:
                return null;
        }
    }

    // Gets the reading order indexes of each relevent segment for the compass's direction and position
    // Indexes greater than 6 refer to the segments of the right digit
    // Returns null if the constraint position or data is an unexpected value
    int[] GetCompassSegmentIndexes()
    {
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
