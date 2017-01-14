# Nancy.Rest.Module

A Nancy Module capable of mounting an annotated interface for [Nancy](http://nancyfx.org).

## Installation

Add [Nancy](http://nancyfx.org), [Nancy.Rest.Module](https://github.com/maxpiva/Nancy.Rest.Module) and [Nancy.Rest.Annotations](https://github.com/maxpiva/Nancy.Rest.Annotations) to your server project.

## Basic Usage

####Create your Server signatures:


```csharp

namespace Nancy.Rest.ExampleServer
{
    [RestBasePath("/api")]
    public interface IExample
    {
        [Rest("Person", Verbs.Get)]
        List<Person> GetAllPersons();
        
        [Rest("Person/{personid}", Verbs.Get)]
        Person GetPerson(int personid);
        
        [Rest("Person", Verbs.Post)]
        bool SavePerson(Person person);

        [Rest("Person/{personid}", Verbs.Delete)]
        bool DeletePerson(int personid);
    }
}
```

####And Your Server Models

```csharp

namespace Nancy.Rest.ExampleServer
{    
    public class Person
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
        
        [Level(1)]
        public bool IsProgrammer { get; set; }
        
        [Tags("Attr")]
        public List<string> Attributes { get; set; }
    }
}

```

The following annotations are provided:

* RestBasePath - The default path using to mount the module in Nancy.
* Rest - Here you can add the route path, the verb, and optionaly the response content-type
* Level - For transversal serialization, when a client sent the **level={number}** as a query parameter, any property in the models with level bigger than the one provided will be not sent to the client.
* Tag - For transversal serialization, when a client sent the **excludetags={comma_separated_tags)** as a query parameter, any property in the model with the tags included will be no sent back to the client.
* Ignore - The property will be ignored.



#### Example Server

##### Your Server Implementation

```csharp

namespace Nancy.Rest.ExampleServer
{
    public class ExampleImplementation
    {
        public List<Person> GetAllPersons
        {
        //TODO        
        }
        
        public Person GetPerson(int personid)
        {
        //TODO                
        }
        
        public bool SavePerson(Person person)
        {
        //TODO
        }
        
        public bool DeletePerson(Person person)
        {
        //TODO
        }
    }
}
```

##### Bootstrapper

```csharp

namespace Nancy.Rest.ExampleServer
{
    public class Bootstrapper : RestBootstrapper
    {
    
    }
}   

```

##### Module 
```csharp

namespace Nancy.Rest.ExampleServer
{   
    public class ExampleModule : RestModule
    {
        public ExampleModule()
        {
            SetRestImplementation(new ExampleImplementation());
        }
    }

}
```

##### Running Nancy as selfhosted

```csharp

    public class Example
    {
        public void Run()
        {
            HostConfiguration config = new HostConfiguration();
            config.UrlReservations.CreateAutomatically = false;
            config.RewriteLocalhost = true;
            NancyHost hostNancy = new Nancy.Hosting.Self.NancyHost(config, new Uri("http://localhost"));
            hostNancy.Start();
        }
    }
}

```

To use the server in C# clients without much trouble and dynamic client proxy generation please continue reading how to use the client in [Nancy.Rest.Client](https://github.com/maxpiva/Nancy.Rest.Client)


## History

**1.4.3-Alpha**: First Release

## Built With

* [Nancy](http://nancyfx.org)
* [JSON.Net](newtonsoft.com/json/) 
* [YAXLib](https://github.com/sinairv/YAXLib)

## Credits

* **Maximo Piva** -[maxpiva](https://github.com/maxpiva)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details


