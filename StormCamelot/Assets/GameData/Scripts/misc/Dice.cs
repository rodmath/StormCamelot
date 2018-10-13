using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Dice
{

	public static int D6 { get { return Random.Range (1, 7); } }

	public static int TwoD6 { get { return D6 + D6; } }

    public static float TwoD6Normalised { get { return (float)(D6 + D6) / 12f; } }
}
