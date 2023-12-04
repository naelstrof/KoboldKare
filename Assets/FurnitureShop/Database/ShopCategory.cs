using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ShopCategory 
{   [SerializeField]
    private string categoryName;
    private List<ShopCategory> subCategories;
    private List<ShopItem> items;

    public ShopCategory(string name){
        categoryName=name;
        subCategories= new List<ShopCategory>();
        items=new List<ShopItem>();
    }   

    public List<ShopCategory> GetSubcategories(){
        return subCategories;
    }
    public List<ShopItem> GetItems(){
        return items;
    }
    public string GetName(){
        return categoryName;
    }
}
