using System.Collections;
using System.Collections.Generic;
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
 * The object for a constraint
 * Consists of the type of constraint and data for the constraint
 */
public class Constraint
{
    ConstraintType constraintType;
    int[] constraintData;

    // Randomly generate a constraint when a new constraint object is created
    public Constraint()
    {
        constraintType = (ConstraintType)Random.Range(0, 9);
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
    public bool NumberPassesConstraint(int number)
    {
        int leftDigit = number / 10;
        int rightDigit = number % 10;
        switch (constraintType)
        {
            case ConstraintType.Gear:
                if (constraintData[0] == 0)
                    return rightDigit > leftDigit;
                else if (constraintData[0] == 1)
                    return leftDigit > rightDigit;
                else
                    return leftDigit == rightDigit;
            default:
                return true;
        }
    }
}
