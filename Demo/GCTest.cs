using System;
using C64Lib;
public class Test
{
    public int Id;

    public Test Child;
}

public class GCTest
{
    static Test s_a1;
    static Test s_a2;
    public static void Two_Instances_First_GCd_Static_Field()
    {
        s_a1 = new Test() { Id = 4 };
        s_a2 = new Test() { Id = 5 };

        var objId = C64.Debug.GetObjectId(s_a1);
        var objId2 = C64.Debug.GetObjectId(s_a2);
        if (!C64.Debug.IsAlive(objId))
        {
            Console.WriteLine("not alive");
            return;
        }
        s_a1 = null;
        GC.Collect();

        if (C64.Debug.IsAlive(objId))
            Console.WriteLine("a1 still alive!");

        if (!C64.Debug.IsAlive(objId2))
            Console.WriteLine("a2 not alive!");

        if (s_a2.Id != 5)
            Console.WriteLine("a2 corrupted");
        C64Lib.C64.SetChar(0, 1, (uint)s_a2.Id);
        s_a1 = s_a2 = null;
    }

    public static void Single_Instance_GC()
    {
        var a = new Test();
        var aid = C64.Debug.GetObjectId(a);
        a = null;
        if (!C64.Debug.IsAlive(aid))
            Console.WriteLine("A IS NOT ALIVE!");
    }

    public static void Change_Ref_Local_Variable()
    {
        var a = new Test();
        var aid = C64.Debug.GetObjectId(a);
        a = new Test();
        if (!C64.Debug.IsAlive(aid))
            Console.WriteLine("A IS NOT ALIVE!");
    }

    public static void Foo(Test t)
    {
        var b = t;
    }
    public static void Passed_As_Parameter_Deferenced()
    {
        var a = new Test();
        var aid = C64.Debug.GetObjectId(a);
        Foo(a);
        a = new Test();
        if (!C64.Debug.IsAlive(aid))
            Console.WriteLine("A IS NOT ALIVE!");
    }

    public static void Array_Root_in_Local_Var()
    {
        var a = new Test[] { new Test() { Id = 23 } };
        var aid = C64.Debug.GetObjectId(a[0]);
        GC.Collect();
        if (!C64.Debug.IsAlive(aid))
            Console.WriteLine("A IS NOT ALIVE!");
        a = null;
        GC.Collect();
        if (C64.Debug.IsAlive(aid))
            Console.WriteLine("A IS STILL ALIVE!");

    }

    public static void Hierarchies_Local_Variable_Root()
    {
        var a1 = new Test() { Id = 1, Child = new Test() };
        var b1 = new Test() { Id = 4 };
        var a2 = new Test() { Id = 2, Child = a1 };
        var b2 = new Test() { Id = 5, Child = b1 };
        var a3 = new Test() { Id = 3, Child = a2 };
        var b3 = new Test() { Id = 6, Child = b2 };

        a1 = a2 = a3 = null;
        // b1 = b2 = null;

        var a1id = C64.Debug.GetObjectId(a1);
        var a2id = C64.Debug.GetObjectId(a2);
        var a3id = C64.Debug.GetObjectId(a3);

        var b1id = C64.Debug.GetObjectId(b1);
        var b2id = C64.Debug.GetObjectId(b2);
        var b3id = C64.Debug.GetObjectId(b3);

        GC.Collect();

        if (b3.Id != 6)
            Console.WriteLine("b3.id != 4");

        if (b3.Child.Id != 5)
            Console.WriteLine("b3.child.id != 5");

        if (b3.Child.Child.Id != 4)
            Console.WriteLine("b3.child.child.id != 4");

        if (!C64.Debug.IsAlive(b1id))
            Console.WriteLine("b1 is not alive!");

        if (!C64.Debug.IsAlive(b2id))
            Console.WriteLine("b2 is not alive!");

        if (!C64.Debug.IsAlive(b3id))
            Console.WriteLine("b3 is not alive!");

        if (C64.Debug.IsAlive(a1id))
            Console.WriteLine("a1 is alive!");

        if (C64.Debug.IsAlive(a2id))
            Console.WriteLine("a2 is alive!");

        if (C64.Debug.IsAlive(a3id))
            Console.WriteLine("a3 is alive!");
    }

    public static void Start()
    {
        Single_Instance_GC();
        Change_Ref_Local_Variable();
        Two_Instances_First_GCd_Static_Field();
        Array_Root_in_Local_Var();
        Hierarchies_Local_Variable_Root();
        Passed_As_Parameter_Deferenced();
    }
}