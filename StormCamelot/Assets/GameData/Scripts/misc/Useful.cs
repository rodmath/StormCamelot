using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Vectrosity;

public static class Useful
{
    public static void DoNothing()
    {
    }

    public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
    {
        IEnumerable<IEnumerable<T>> emptyProduct =
            new[] { Enumerable.Empty<T>() };
        return sequences.Aggregate(
            emptyProduct,
            (accumulator, sequence) =>
            from accseq in accumulator
            from item in sequence
            select accseq.Concat(new[] { item }));
    }


    public static string Pluralise(int num, string thing)
    {
        //TODO - do all these rules:
        //http://www.firstschoolyears.com/literacy/word/other/plurals/resources/rules.htm
        //		System.Data.Entity.Design.PluralizationServices


        if (num == 1)
        {
            return "a " + thing;
        }
        else
        {
            string lastChar = thing.Right(1);
            string last2Char = thing.Right(2);
            if ("s, x, z".Contains(lastChar) || "ch, sh".Contains(last2Char))
            {
                return num + " " + thing + "es";
            }
            else if (lastChar == "y")
                return num + " " + thing.Left(thing.Length - 1) + "ies";
            else
                return num + " " + thing + "s";
        }
    }

    public static string GetLetter()
    {
        // This method returns a random lowercase letter.
        // ... Between 'a' and 'z' inclusize.
        int num = Random.Range(0, 26); // Zero to 25
        char let = (char)('a' + num);
        return let.ToString();
    }



    public static Bounds FullBounds(GameObject obj)
    {
        SpriteRenderer[] spriteRends = obj.GetComponentsInChildren<SpriteRenderer>();
        if (spriteRends.Length > 0)
        {
            Bounds bound = spriteRends[0].bounds;
            foreach (SpriteRenderer sr in spriteRends)
            {
                bound.Encapsulate(sr.bounds);
            }
            return bound;
        }

        MeshRenderer[] meshRends = obj.GetComponentsInChildren<MeshRenderer>();
        if (meshRends.Length > 0)
        {
            Bounds bound = meshRends[0].bounds;
            foreach (MeshRenderer mr in meshRends)
            {
                bound.Encapsulate(mr.bounds);
            }
            return bound;
        }

        Debug.LogWarning(obj.name + " has no sprite or mesh renderers");
        return new Bounds();
    }

    public static Rect GetScreenRect(this RectTransform rectTransform, Canvas canvas)
    {

        Vector3[] corners = new Vector3[4];
        Vector3[] screenCorners = new Vector3[2];

        rectTransform.GetWorldCorners(corners);

        if (canvas != null && (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace))
        {
            screenCorners[0] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[1]);
            screenCorners[1] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[3]);
        }
        else
        {
            screenCorners[0] = RectTransformUtility.WorldToScreenPoint(null, corners[1]);
            screenCorners[1] = RectTransformUtility.WorldToScreenPoint(null, corners[3]);
        }

        screenCorners[0].y = Screen.height - screenCorners[0].y;
        screenCorners[1].y = Screen.height - screenCorners[1].y;

        return new Rect(screenCorners[0], screenCorners[1] - screenCorners[0]);
    }

    public static Vector2 Perpendicular(Vector2 original)
    {
        //To create a perpendicular vector switch X and Y, then make Y negative
        float x = original.x;
        float y = original.y;

        y = -y;

        return new Vector2(y, x);
    }


    //x must be 0-1.  Returns a value in range 0-1, with x=0.5 being max (1) and x=0 or x=1 both retuning 0
    public static float taperFunc(float x, float steepness = 1)
    {
        float y = (1f - Mathf.Pow(2f * x - 1f, 2f * steepness));
        return y;
    }


    public static float RandomRange(Vector2 range)
    {
        return Random.Range(range.x, range.y);
    }


    public static Vector2 RandomVector2(float minInc, float maxExl)
    {
        return new Vector2(Random.Range(minInc, maxExl), Random.Range(minInc, maxExl));
    }


    public static bool RandomBoolean()
    {
        return (Random.value > 0.5f);
    }


    public static int RandomSign()
    {
        if (RandomBoolean())
            return 1;
        else
            return -1;
    }

    public static Vector2 RandomVector2Direction()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float x = Mathf.Cos(angle);
        float y = Mathf.Sin(angle);

        return new Vector2(x, y);
    }


    public static Color RandomColor(float alpha = 1f)
    {
        float r = Random.Range(0, 1f);
        float g = Random.Range(0, 1f);
        float b = Random.Range(0, 1f);

        return new Color(r, g, b, alpha);
    }


    public static Color RandomPastelColor(float alpha = 1f)
    {
        // to create lighter colours:
        // take a random integer between 0 & 128 (rather than between 0 and 255)
        // and then add 127 to make the colour lighter
        byte r = (byte)(Random.Range(0, 128) + 127);
        byte g = (byte)(Random.Range(0, 128) + 127);
        byte b = (byte)(Random.Range(0, 128) + 127);

        return new Color(r, g, b, alpha);
    }

    public static Vector2 Vector2FromAngle(float degrees)
    {
        degrees *= Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(degrees), Mathf.Sin(degrees));
    }


    public static float DegreesForVector(Vector3 vector)
    {
        return Mathf.Atan2(vector.normalized.y, vector.normalized.x) * Mathf.Rad2Deg;
    }

    public static int NextIndex<T>(List<T> list, int index, bool backwards = false)
    {
        if (!backwards)
        {
            index = index + 1;
            while (index >= list.Count)
                index = index - list.Count;

            return index;
        }
        else
        {

            index = index - 1;
            while (index < 0)
                index = index + list.Count;

            return index;
        }
    }


    public static T PickRandom<T>(List<T> list)
    {
        if (list.Count == 0)
            return default(T);

        int i = Random.Range(0, list.Count);
        return list[i];
    }


    public static void DrawCross(Vector3 p, float radius, Color col)
    {
        Vector3 n = new Vector3(0, radius, 0);
        Vector3 e = new Vector3(radius, 0, 0);
        VectorLine v = VectorLine.SetLine(col, p - n, p + n);
        VectorLine h = VectorLine.SetLine(col, p - e, p + e);
        v.Draw();
        h.Draw();
    }

    public static bool hasLOSOLD(Vector2 a, Vector2 b, GameObject target, int layersToIgnore, bool debugDraw = false)
    {
        //		Debug.Log ("testing " + a + " and " + b + " on " + target.name);
        bool hasLOS = false;
        RaycastHit2D hitPoint = Physics2D.Raycast(a, b - a, (b - a).magnitude, ~layersToIgnore);
        if (hitPoint.collider == null)
        {
            hasLOS = true;
        }
        else
        {
            GameObject hitObj = hitPoint.collider.gameObject;
            bool isTarget = (hitObj == target);

            //if this isn't are target, check every parent until the root
            while (!isTarget && hitObj.transform.parent != null)
            {
                hitObj = hitObj.transform.parent.gameObject;
                isTarget = (hitObj == target);
            }

            if (isTarget)
                hasLOS = true;
            else
                hasLOS = false;
        }

        if (debugDraw)
        {
            Color col = Color.green;
            Vector3 a3 = new Vector3(a.x, a.y, 0);
            Vector3 b3 = new Vector3(b.x, b.y, 0);
            Vector3 h3 = b3;
            if (!hasLOS)
                h3 = new Vector3(hitPoint.point.x, hitPoint.point.y, 0);

            Debug.DrawLine(a3, h3, new Color(0, 1, 0, 0.4f), 0.5f);
            Debug.DrawLine(h3, b3, new Color(1, 0, 0, 0.4f), 0.5f);
        }


        return hasLOS;
    }


    //public LayerMask layerToNotHit;
    //hitPoint2D = Physics2D.Raycast (transform.position, step.normalized, step.magnitude * RaycastAdvance, ~layerToNotHit);
    //vp.layerToNotHit = 1 << gameObject.layer;

    public static bool hasLOS(GameObject source, GameObject target, bool debugDraw = false, int layerMask = Physics2D.AllLayers)
    {
        bool hasLOS = false;
        RaycastHit2D[] hitPoints = new RaycastHit2D[1];
        Vector2 a = source.transform.position;
        Vector2 b = target.transform.position;
        Vector2 direction = b - a;
        int hits = 0;

        //if our source has a collider we will use the raycast from that
        Collider2D collider = source.GetComponentInChildren<Collider2D>();
        if (collider)
            hits = collider.Raycast(direction, hitPoints, direction.magnitude, layerMask);
        else
            hits = Physics2D.RaycastNonAlloc(source.transform.position, direction, hitPoints, direction.magnitude, layerMask);



        if (hits == 0)
        {
            hasLOS = true;
        }
        else
        {
            GameObject hitObj = hitPoints[0].collider.gameObject;
            bool isTarget = (hitObj == target);

            //if this isn't are target, check every parent until the root
            while (!isTarget && hitObj.transform.parent != null)
            {
                hitObj = hitObj.transform.parent.gameObject;
                isTarget = (hitObj == target);
            }

            if (isTarget)
                hasLOS = true;
            else
                hasLOS = false;
        }

        if (debugDraw)
        {
            Color col = Color.green;
            Vector3 a3 = new Vector3(a.x, a.y, 0);
            Vector3 b3 = new Vector3(b.x, b.y, 0);
            Vector3 h3 = b3;
            if (!hasLOS)
                h3 = new Vector3(hitPoints[0].point.x, hitPoints[0].point.y, 0);

            VectorLine.SetLine(new Color(0, 1, 0, 0.4f), a3, h3);
            VectorLine.SetLine(new Color(1, 0, 0, 0.4f), h3, b3);

        }

        return hasLOS;
    }


    public static bool IsPointingAt(Transform source, Transform target, float tolerence)
    {
        Vector3 a = source.right;
        Vector3 b = (target.position - source.position);

        float dot = Vector3.Dot(a, b.normalized);
        float tolerenceDot = 1f - tolerence;

        return dot > tolerenceDot;

    }


    public static Color32 AverageColorFromTexture(Texture2D tex)
    {
        Color32[] texColors = tex.GetPixels32();
        int total = texColors.Length;

        float r = 0;
        float g = 0;
        float b = 0;

        for (int i = 0; i < total; i++)
        {
            r += texColors[i].r;
            g += texColors[i].g;
            b += texColors[i].b;
        }
        return new Color32((byte)(r / total), (byte)(g / total), (byte)(b / total), 0);
    }


    public static Color colourAtPoint(Texture2D tex, Vector2 p)
    {
        Color col = tex.GetPixel((int)p.x, (int)p.y);
        return col;

    }

    public static float Area(Vector3[] verts)
    {
        List<Vector3> vertices = new List<Vector3>();
        for (int i = 0; i < verts.Length; i++)
            vertices.Add(verts[i]);

        return Area(vertices);
    }



    public static float Area(List<Vector3> vertices)
    {
        vertices.Add(vertices[0]);
        return Mathf.Abs(vertices.Take(vertices.Count - 1).Select((p, i) => (p.x * vertices[i + 1].y) - (p.y * vertices[i + 1].x)).Sum() / 2);
    }




    public static List<Vector3> Segment(Vector3 a, Vector3 b, int segments)
    {
        List<Vector3> points = new List<Vector3>();

        for (int i = 0; i < segments + 1; i++)
        {
            points.Add(Vector3.Lerp(a, b, (float)i / (float)segments));
        }

        return points;
    }



    public static void Segment(Vector3 a, Vector3 b, int segments, List<Vector3> points)
    {
        if (points.Count != segments + 1)
        {
            Debug.Log("Incorrect number of entries - not updating");
        }

        for (int i = 0; i < segments + 1; i++)
        {
            points[i] = Vector3.Lerp(a, b, (float)i / (float)segments);
        }

    }


    public static int layerMask2Layer(LayerMask mask)
    {
        int layerNumber = 0;
        int layer = mask.value;
        while (layer > 0)
        {
            layer = layer >> 1;
            layerNumber++;
        }

        return layerNumber - 1;
    }


    public static bool IsInLayerMask(int layer, LayerMask layermask)
    {
        return layermask == (layermask | (1 << layer));
    }


}













public static class StringExtensions
{
    public static string Right(this string str, int length)
    {
        return str.Substring(str.Length - length, length);
    }

    public static string Left(this string str, int length)
    {
        return str.Substring(0, length);
    }

    public static string RP(this string phraseList, char delimiter = ',', bool startCap = false)
    {
        return RandomPhrase(phraseList, delimiter, startCap);
    }

    public static string RandomPhrase(this string phraseList, char delimiter = ',', bool startWithCapital = false)
    {
        string[] phrases = phraseList.Split(delimiter);

        string phrase = phrases[Random.Range(0, phrases.Length - 1)].Trim();

        if (startWithCapital)
            phrase = phrase[0].ToString().ToUpper() + phrase.Substring(1);

        return phrase;
    }

}



public static class FloatExtensions
{

    //Take the decimal part of a float and convert it to the percentage through the day, 0 = 00:00, 1 = 24:00
    public static string ToTimeOnlyString(this float aDayTime)
    {
        float dayPercent = aDayTime % 1f;
        int hrs = Mathf.FloorToInt(dayPercent * 24f);
        int mins = Mathf.FloorToInt(dayPercent * 60f * 24f) - (hrs * 60);
        return hrs.ToString("00") + ":" + mins.ToString("00");
    }

    public static string ToDayTimeString(this float aDayTime)
    {
        float dayPercent = aDayTime % 1f;
        int days = Mathf.FloorToInt(aDayTime);
        int hrs = Mathf.FloorToInt(dayPercent * 24f);
        int mins = Mathf.FloorToInt(dayPercent * 60f * 24f) - (hrs * 60);
        return days.ToString("00") + ":" + hrs.ToString("00") + ":" + mins.ToString("00");
    }


    public static string ToFauxCoord(this float number)
    {
        // 12345.6 => 1° 23' 45"

        string s = ((int)number).ToString("D6");
        string deg = s.Left(s.Length - 4);
        string min = s.Right(4).Left(2);
        string sec = s.Right(2);

        return deg + "°" + min + "'" + sec + @"""";
    }


    public static float Clamp(this float number, float min, float max)
    {
        if (min < max)
            return Mathf.Clamp(number, min, max);
        if (max < min)
            return Mathf.Clamp(number, max, min);
        else
            return number;

    }

    public static float Abs(this float number)
    {
        return Mathf.Abs(number);
    }

    public static float Squared(this float number)
    {
        return (number * number);
    }

    public static bool RoughlyEquals(this float thisFloat, float otherFloat)
    {
        return (Mathf.Abs(thisFloat - otherFloat) < 0.0001f);
    }
}



public static class Vector2Extensions
{

    public static bool Contains(this Vector2 v, float number, bool inclusive = true)
    {
        if (inclusive)
        {
            return (number >= v.x || number <= v.y);
        }
        else
        {
            return (number > v.x || number < v.y);
        }
    }

    public static Vector2 PerpendicularClockwise(this Vector2 vector2)
    {
        return new Vector2(-vector2.y, vector2.x);
    }

    public static Vector2 PerpendicularCounterClockwise(this Vector2 vector2)
    {
        return new Vector2(vector2.y, -vector2.x);
    }
}


public static class Vector3Extensions
{

    public static Vector3 ClampMagnitude(this Vector3 v, float min, float max)
    {
        float len = v.magnitude;
        if (len > max)
            return v.normalized * max;
        else if (len < min)
            return v.normalized * min;
        else
            return v;
    }

}





public static class ColorExtensions
{
    public static Color InvertColor(this Color color)
    {
        return new Color(1.0f - color.r, 1.0f - color.g, 1.0f - color.b, color.a);
    }


    public static Color Complementary(this Color color)
    {
        HSLColor hsl = color;

        float compHue = hsl.h;
        compHue = (compHue + 180f) % 360f;

        HSLColor comp = new HSLColor(compHue, hsl.s, hsl.l, hsl.a);

        return comp.ToRGBA();
    }

    public static Color Contrast(this Color color)
    {
        Color y = new Color(
                      color.r > 0.5 ? 0 : 1,
                      color.g > 0.5 ? 0 : 1,
                      color.b > 0.5 ? 0 : 1
                  );
        return y;
    }


    public static Color ChangeAlpha(this Color color, float newAlpha)
    {
        return new Color(color.r, color.g, color.b, newAlpha);
    }

    public static string ToHex(this Color color)
    {
        Color32 color32 = color;
        string hex = color32.r.ToString("X2") + color32.g.ToString("X2") + color32.b.ToString("X2");
        return hex;
    }
}


public static class Rigidbody2DExtension
{

    public static float AddExplosionForce(this Rigidbody2D rb, float explosionForce, Vector2 explosionPosition, float explosionRadius, float upwardsModifier = 0.0F, ForceMode2D mode = ForceMode2D.Force)
    {
        var explosionDir = rb.position - explosionPosition;
        var explosionDistance = explosionDir.magnitude;

        // Normalize without computing magnitude again
        if (upwardsModifier == 0)
            explosionDir /= explosionDistance;
        else
        {
            // From Rigidbody.AddExplosionForce doc:
            // If you pass a non-zero value for the upwardsModifier parameter, the direction
            // will be modified by subtracting that value from the Y component of the centre point.
            explosionDir.y += upwardsModifier;
            explosionDir.Normalize();
        }

        float proportionalEffect = (1 - explosionDistance);

        rb.AddForce(Mathf.Lerp(0, explosionForce, proportionalEffect) * explosionDir, mode);

        return proportionalEffect;
    }
}


public static class TransformExtensions
{
    public static void SetLayer(this Transform trans, int layer)
    {
        trans.gameObject.layer = layer;
        foreach (Transform child in trans)
            child.SetLayer(layer);
    }
}



public static class BoundsExtensions
{
    public static Vector2 ClosestPointOnBounds(this Bounds bounds, Vector2 p)
    {
        //If point is outside bounds, just use this:
        if (!bounds.Contains(p))
            return bounds.ClosestPoint(p);

        //Point is inside bounds, so check distance to find closest point
        List<Vector2> boundPos = new List<Vector2>();
        boundPos.Add(new Vector2(bounds.min.x, p.y));
        boundPos.Add(new Vector2(bounds.max.x, p.y));
        boundPos.Add(new Vector2(p.x, bounds.min.y));
        boundPos.Add(new Vector2(p.x, bounds.max.y));

        Vector2 closestPos = boundPos[0];
        foreach (Vector2 bpos in boundPos)
        {
            if (Vector2.Distance(p, bpos) < Vector2.Distance(p, closestPos))
                closestPos = bpos;
        }

        return closestPos;
    }


    public static Vector3 RandomPointInside(this Bounds bounds)
    {

        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        float z = Random.Range(bounds.min.z, bounds.max.z);

        return new Vector3(x, y, z);
    }
}


public static class RectExtensions
{
    public static Vector2 RandomPointInside(this Rect rect)
    {

        float x = Random.Range(rect.min.x, rect.max.x);
        float y = Random.Range(rect.min.y, rect.max.y);

        return new Vector2(x, y);
    }

}

public static class List
{
    public static T Last<T>(this IList<T> list)
    {
        if (list == null)
            throw new System.ArgumentNullException("list");
        if (list.Count == 0)
            throw new System.ArgumentException(
                "Cannot get last item because the list is empty");

        int lastIdx = list.Count - 1;
        return list[lastIdx];
    }

    public static void RemoveLast<T>(this IList<T> list)
    {
        if (list == null)
            throw new System.ArgumentNullException("list");
        if (list.Count == 0)
            throw new System.ArgumentException(
                "Cannot remove last item because the list is empty");

        int lastIdx = list.Count - 1;
        list.RemoveAt(lastIdx);
    }

}

