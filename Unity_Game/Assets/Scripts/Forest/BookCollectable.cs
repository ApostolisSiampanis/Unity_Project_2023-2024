using Farm.Scripts.Interaction_System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookCollectable : Collectable
{
    // Start is called before the first frame update
    void Start()
    {
        itemType = Item.ItemType.Book;
    }

}
