> Proyecto para poder gestionar de mejor forma los scripts de base de datos.
> Proyecto personal, retomado después de muucho tiempos.
> Puede mejorar ... disculpen los errores/problemas que encuentren.

# Transversal.Util.BaseDBUp

Se conforma de dos partes:

* BaseDBUp
* TestDBUp

La primera consiste en la base del código, que se construye a partir de otras iniciativas (ver referencias), la segunda es una aplicación de consola que sirve para probar el funcionamiento, o incluso se puede extender para poder usarse de forma de ordenar los scripts sin necesidad de que se ejecuten de forma automática.

## BaseDBUp

Se basa en las siguientes inciciativas y desarrollos:

* [DbUp](https://dbup.github.io/)
* [https://www.nuget.org/packages/dbup-sqlserver](https://www.nuget.org/packages/dbup-sqlserver)
* [https://www.nuget.org/packages/dbup-postgresql](https://www.nuget.org/packages/dbup-postgresql)

> **La implementación esta pensada para ser usasda con SqlServer y PostgresSql**

Se consideran los siguientes argumentos:
 * Utilizar el string de conexión a la base de datos.
 * La ruta donde se encuentran los scripts a ser utilizados.
 * El patrón de inicio de los scripts (*Por ejemplo, todos los scripts deben comenzar con un patrón, para así poder guardar otros que no se necesitan que se apliquen al momento que la plaicación se levante*).
 * Luego considera una sera de patrones de terminación, para identificar el tipo de scripts y dejar algunos afuera en caso de que se necesite:
   * Los scripts de estructura son aquellos con terminación **-bd.sql**. Si se consideran se debe indicar *true*, en caso contrario *false*.
   * Los scripts para revertir cambios anteriores son aquellos con terminación **-rv.sql**. Si se consideran se debe indicar *true*, en caso contrario *false*.
   * Los scripts para cargar datos son aquellos con terminación **-data.sql**. Si se consideran se debe indicar *true*, en caso contrario *false*.
   * Los scripts con datos para desarrollo (para ambientes no productivos) son aquellos con terminación **-dev.sql**. Si se consideran se debe indicar *true*, en caso contrario *false*.
   * Los scripts con procedimientos o funciones son aquellos de terminación **-sp.sql**. Si se consideran se debe indicar *true*, en caso contrario *false*. 

## TestDBUp

Es una aplicación de consola que permite probar la implementación de forma simplificada (sin todos los parámetros antes descritos). Utiliza para las pruebas un motor Sql Server 2017, para ello se utilizará [Docker](https://www.docker.com/) y la respectiva [imagen 'mssql/server:2017'](https://hub.docker.com/_/microsoft-mssql-server).

### Crear un contenedor

> Paso opcional, si no se tiene ya una instancia, o no se quiere usar la existente.

Se pueda hacer mediante la siguiente instrucción:

```bat
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Bdpass001." -e 'MSSQL_PID=Enterprise' -p 1433:1433 --name sql1 -h sql1 -d mcr.microsoft.com/mssql/server:2017-latest 
```
Se puede ingresar a la base de datos con la siguiente instrucción:

```bat
docker exec -it sql1 /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Bdpass001.
```
Notar que:
1. El usuario es *sa*, lo cual se **debe** cambiar si se desea ocupar, es decir, ocupar otro usuario.
2. La contraseña de acceso es **Bdpass001.**
3. La instancia se mapea al mismo puero, es decir **1433**, puede ser necesario cambiarla en caso de que se tenga el puerto ocupado.

### Crear una base de datos de pruebas.

Dentro de la base de datos se recomienda crear una base de datos para las pruebas, por ejemplo:

```sql
create database EjemploDbUp
```

### Probar aplicación de consola

Para probar la aplicación de consola se debe ejecutar:

```bat
dotnet run "Server=host.docker.internal,1433;Database=EjemploDBUp;Trusted_Connection=false;MultipleActiveResultSets=true;User Id=sa;Password=Bdpass001.", "./Scripts", "TEST"
```
Donde:

* _"Server=host.docker.internal,1433;Database=EjemploDBUp;Trusted_Connection=false;MultipleActiveResultSets=true;User Id=sa;Password=Bdpass001."_ : Corresponde al string de conexión a la base de datos con los parámetros indicados en los puntos anteriores (se estos se cambiaron, se debe hacer lo mismo en este punto).
* _"./Scripts"_ : Corresponde a la ruta donde vienen los scripts de prueba. **Notar que se utiliza un identificador numérico, esto se hace para apreciar el orden de mejor manera (recomendable, mas no obligatorio)**.
* _"TEST"_ : Corresponde al prefijo que se deberá considerar para utilizar los archivos (ver [ejemplo](./TestDBUp/Scripts/TEST-000001-Crear%20tabla-bd.sql))

> **IMPORTANTE!** Una vez ejecutado un script, no se volverá ejecutar aunque cambie su contenido. Cualquier cambio (por ejemplo, una corrección a un script aplicado) se debe realizar en un nuevo script (_por eso es que ayuda a mantener un historial de los cambios que se han realizado_).

Se debería obtener la siguiente salida.

```bat
TO DB: Server=host.docker.internal,1433;Database=EjemploDBUp;Trusted_Connection=false;MultipleActiveResultSets=true;User Id=sa;Password=Bdpass001.
FROM SCRIPTS: ./Scripts
PATTERN: TEST
Master ConnectionString => Data Source=host.docker.internal,1433;Initial Catalog=master;Integrated Security=False;User ID=sa;Password=******;MultipleActiveResultSets=True
Beginning transaction
Checking whether journal table exists..
Journal table does not exist
Beginning transaction
Beginning database upgrade
Checking whether journal table exists..
Journal table does not exist
Executing Database Server script 'TEST-000001-Crear tabla-bd.sql'
Checking whether journal table exists..
Creating the [SchemaVersions] table
The [SchemaVersions] table has been created
Upgrade successful
Ok
```

Como resultado, se debería crear la tabla **A** indicado en el [script](/TestDBUp/Scripts/TEST-000001-Crear%20tabla-bd.sql) y una tabla llamada **SchemaVersions** que solo se creará la primera vez (cuando no exista) y que almacenará los registros de los parches aplicados. 