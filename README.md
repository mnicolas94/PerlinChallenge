Este proyecto fue creado para el #ThePerlinChallenge organizado en el grupo de telegram Matcom Game Development https://t.me/matcomgamefestival. Este evento se desarrolló del 18 de mayo al 1 de junio de 2020.

En el proyecto está implementado un mecanismo para generar terreno a partir de sonido. La máxima amplitud del sonido en cada frame es usada para determinar la altura máxima de las montañas, mientras que la frecuencia fundamental y los armónicos del sonido son usados para determinar la posición de las montañas en el eje x. Cada montaña es una campana de gauss donde la media corresponde a la frecuencia fundamental o armónico y la varianza es un parámetro que se puede modificar. El ruido perlin (Perlin Noise) es usado para "suavizar" el terreno y para darle variedad a las zonas que no tienen montañas. Además, se aplicó Perlin noise también en la creación de un río que corre por el terreno. La posición del centro del río en el eje Z se determina a partir de 3 elementos. El primero es el Perlin noise como se dijo anteriormente. El segundo es una especie de fuerza que aleja al río de las montañas. Y el tercero es otra fuerza que acerca al río al centro del terreno. El sonido a partir del cual se genera el terreno se puede obtener ya sea a partir de un fichero de audio (una canción si lo desea) o a partir de la entrada del micrófono.

Todos los assets (que no son muchos) fueron creados durante la duración del evento, excepto los que se encuentran dentro de la carpeta 3rdParty.

Como documentación de apoyo para la realización del proyecto se consultaron los siguientes links:

- https://medium.com/giant-scam/algorithmic-beat-mapping-in-unity-intro-d4c2c25d2f27 Este link es de una serie de artículos sobre beat mapping. Está de lo más interesante, lo recomiendo incluso como curiosidad para el que no tenga pensado hacer nada de eso. De aquí saqué cómo calcular la frecuencia fundamental y los armónicos. 
- http://wiki.unity3d.com/index.php/CreatePlane?_ga=2.188882024.255555665.1591037632-1435871360.1585011956 De aquí saqué cómo crear y manipular el mesh del terreno. 
- https://gametorrahod.com/various-noise-functions/ De aquí saqué cómo usar el perlin noise vitaminado de Unity Mathematics. Gracias a Pedro @UltraKelevra. 

