using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFixable
{
    bool CanBeFixed(Inventory inventory);
    void Fix(Inventory inventory);
}
