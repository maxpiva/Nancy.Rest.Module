# Nancy.Rest.Module

A Nancy Module capable of mounting an annotated interface and provides transversal filtering capabilities.

## Installation

* Add [Nancy](http://nancyfx.org), [Nancy.Rest.Module](https://github.com/maxpiva/Nancy.Rest.Module) and [Nancy.Rest.Annotations](https://github.com/maxpiva/Nancy.Rest.Annotations) to your server project.

Or 

* Add the Nuget package [Nancy.Rest.Module](https://www.nuget.org/packages/Nancy.Rest.Module/)


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

The following annotations exists:

* RestBasePath - The default path using to mount the module in Nancy.
* Rest - Here you can add the route path, the verb, and optionally the response content-type in case of streams.
* Level - For transversal serialization, when a client sent the **level={number}** as a query parameter, any property in the models with level bigger than the one asked by the client, will be not sent to the client.
* Tag - For transversal serialization, when a client sent the **excludetags={comma_separated_tags)** as a query parameter, any property in the model with the tags included will be not serialized and sent back to the client.
* Ignore - The property will be not serialized and sent back to the client.



#### Example Server

##### Your Server Implementation

```csharp

namespace Nancy.Rest.ExampleServer
{
    public class ExampleImplementation : IExample
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

##### Running Nancy selfhosted

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



###To use the server in C# clients without much trouble and dynamic client proxy generation please continue reading how to use the client in [Nancy.Rest.Client](https://github.com/maxpiva/Nancy.Rest.Client)


## History

**1.4.3-Beta**: Removed bugs, Added StreamWithResponse class, for finetunnig Stream responses, published nugets.
**1.4.3-Alpha**: First Release

##WARNING

THIS IS AN BETA VERSION, so far it works on my machine ;)

## TODO

* Claim/Role based transversal filtering
* Swagger!
* Squash Bugs
* Squash More Bugs
* Pray
* Change Bugs issues to features
* TODO :P

##MAYBE

* Added secure module and secure client, wrapping request and responses models with signatures.


## Built With

* [Nancy](http://nancyfx.org)
* [JSON.Net](http://newtonsoft.com/json/) 

## Credits

* **Maximo Piva** -[maxpiva](https://github.com/maxpiva)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details


