# Kerbal Space Program 2
## Vue d'ensemble

À première vue, le jeu semble être une vulgaire copie de la première version de KSP avec de meilleurs graphismes, mais nécessitant des ressources excessives pour fonctionner et une physique qui n'est pas au rendez-vous. Mais il n'en est rien, ce n'est que la vue du joueur. Il semblerait que nous n'ayons accès qu'à une petite partie du jeu, et pas à toutes les fonctionnalités, peut-être pour éviter d'avoir une mauvaise première image des nouveautés.

## Vue du code

Le code de KSP 1 est assez désordonné. On sent le côté jeu indépendant qui est devenu par la suite, avec l'ajout de couches de patchs correctifs, un jeu plus sérieux. Néanmoins, le jeu garde une base qui n'était pas prévue pour le modding, encore moins pour du multijoueur ou l'ajout de voyage interstellaire.

KSP 2 est une refonte complète du code de KSP 1, avec une structure pensée dès le départ pour l'ajout du modding en profondeur et du multijoueur. Des petits indices subtils sont visibles par les joueurs, comme la création d'une agence spéciale à chaque partie que l'on souhaite créer. 
Il intègre aussi déjà la prochaine mise à jour majeure **La Science**. Le code est clairement déjà présent dans le jeu, avec l'arbre de recherche, les activations, etc. Je n'ai pas eu le temps de vérifier si les interfaces utilisateur et le modèle des bâtiments étaient présents ou non.

### Spéculation de MrCubee d'après le code incomplet

Je pense qu'en multijoueur, chaque joueur aura son agence spatiale avec ses missions propres, et que nous pourrions voir des systèmes "d'appel d'offres" et de collaboration ou de concurrence entre agences.

## Le modding

KSP2 souhaite simplifier la création de mods à sa communauté. Il proposera un équivalent du GameData de KSP1 en même temps que l'ajout du multijoueur. Le code étant déjà présent et fonctionnel en partie, on peut déjà remarquer que KSP2 proposera deux manières de faire des mods, une pour les plus expérimentés en C# et une autre pour les novices/débutants en Lua. Les deux types de mods pourront interagir dans le même environnement. Les mods Lua seront chargés grâce à la bibliothèque MoonSharp déjà présente dans le code du jeu. D'après le code, on peut voir 2 autres types de mod : un type ContentOnly, ce qui signifie du contenu sans code exécutable, et de type Shakespeare, mais je ne sais pas encore ce que c'est.

## Voici les sources que j'ai utilisées
### D'après la bibliothèque dynamique *KSP2_x64_Data\Assembly-CSharp.dll*

#### [Type de mods que KSP2 mettra en place](https://github.com/MrCubee/KSP2-Info/blob/master/src/KSP/Modding/KSP2ModType.cs)
#### [Instance d'une partie de KSP2](https://github.com/MrCubee/KSP2-Info/blob/master/src/KSP/Game/GameInstance.cs)
#### [Gestion des recherches qui sera accessible à la prochaine mise à jour de KSP2 qui ajoutera le mode Science, il est pour l'instant non accessible](https://github.com/MrCubee/KSP2-Info/blob/master/src/KSP/Research/ResearchManager.cs)
#### [Technologie débloquable qui sera accessible à la prochaine mise à jour de KSP2 qui ajoutera le mode Science, il est pour l'instant non accessible](https://github.com/MrCubee/KSP2-Info/blob/master/src/KSP/Research/Technology.cs)
