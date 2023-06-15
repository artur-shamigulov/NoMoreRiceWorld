# No more rice world

## Introduction
This is a mod for RimWorld Game. It aims to make food variety a necessity for both mental and physical health. When pawns eat food with different ingredients, their mood and performance improve. When pawns do not get enough food variety, the opposite happens: their mood and performance worsen.
Pawns need a variety of nutrients to stay healthy and perform their best. These nutrients include proteins, carbohydrates, and vitamins.
Proteins are found in meat, animal products, and some plants.
Carbohydrates are the body's main source of energy.Carbohydrates can be found in plant foods like rice, corn and potatoes.
Vitamins are essential for many bodily functions, such as maintaining a healthy immune system, producing blood cells, and protecting against disease. Vitamins can be found in a variety of foods, including animal products and berries.
In addition to providing your pawns with the nutrients they need, it is also important to provide them with a variety of food. Pawns will not be happy if they are only ever given rice to eat. A mod has added a new need called "Food Variety," and to keep this need high, you should provide your pawns with different meals made from different ingredients.
You do not need to manually manage your pawns' nutritional needs. The food searching algorithm has been patched to search for food sources that satisfy their nutritional and food variety needs. You only need to provide your pawns with a variety of food sources. The food searching algorithm will then prioritize the food sources that provide the nutrients that your pawns need.

It is possible to modify the ThingDef (a data structure that defines a thing in RimWorld) to increase or decrease the amount of elements that is gained when consuming the thing:
```
<Operation Class="PatchOperationAddModExtension">
    <xpath>Defs/ThingDef[defName="RawFungus"]</xpath>
    <value>
        <li Class="NoMoreRiceWorld.ElementsDefModExtension">
            <Vitamines>0</Vitamines>
            <Carbohydrates>0.5</Carbohydrates>
            <Proteins>0.5</Proteins>
        </li>
    </value>
</Operation>
```
For more examples of how to patch ThingDef, see the file `Patches/VanillaFoodPatches.xml`.