using System;
using C64Lib;
public class Test
{
    public int Id;
    public Test Child;
}

public class GCTest
{
    static Test a1 = new Test();
    static Test a2;
    public static void Two_Instances_First_GCd_Static_Field()
    {
        a1 = new Test() { Id = 4 };
        a2 = new Test() { Id = 5 };

        var objId = C64.Debug.GetObjectId(a1);
        var objId2 = C64.Debug.GetObjectId(a2);
        if (!C64.Debug.IsAlive(objId))
        {
            Console.WriteLine("not alive");
            return;
        }
        a1 = null;
        GC.Collect();

        if (C64.Debug.IsAlive(objId))
            Console.WriteLine("a1 still alive!");

        if (!C64.Debug.IsAlive(objId2))
            Console.WriteLine("a2 not alive!");

        if (a2.Id != 5)
            Console.WriteLine("a2 corrupted");
        C64Lib.C64.SetChar(0, 1, (uint)a2.Id);
        a1 = a2 = null;
    }
    public static void Start()
    {
        Two_Instances_First_GCd_Static_Field();
        var a1 = new Test() { Id = 1, Child = new Test() };
        var b1 = new Test() { Id = 4 };
        var a2 = new Test() { Id = 2, Child = a1 };
        var b2 = new Test() { Id = 5, Child = b1 };
        var a3 = new Test() { Id = 3, Child = a2 };
        var b3 = new Test() { Id = 6, Child = b2 };

        a1 = a2 = a3 = null;
        b1 = b2 = null;

        GC.Collect();

        if (b3.Id != 6)
            Console.WriteLine("b3.id != 4");

        if (b3.Child.Id != 5)
            Console.WriteLine("b3.child.id != 5");

        if (b3.Child.Child.Id != 4)
            Console.WriteLine("b3.child.child.id != 4");

        Console.WriteLine("finished");
    }
}