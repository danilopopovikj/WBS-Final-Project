using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;

namespace WBS_Testing_01
{
    class Program
    {
        static void Main(string[] args)
        {
            //ConvertJsonRecipesToJsonRdf();
            
            try
            {
                TripleStore tripleStore = new TripleStore();
                JsonLdParser parser = new JsonLdParser();
                //Load using Filename
                parser.Load(tripleStore, @"D:\Finki\Web based systems\Project\output.jsonld");

                var graph = tripleStore.Graphs.First();
                //CompressingTurtleWriter writer = new CompressingTurtleWriter();

                //Save to a File
                //writer.Save(graph, "Example.ttl");
                Console.WriteLine("Please enter an ingridient:");
                var input = Console.ReadLine();
                var inputIngridients = input.Split(',');

                string x = "x";
                string y = "y";
                var queryBuilder =
                    QueryBuilder
                    .Select(new string[] { x })
                    .Where(
                        (triplePatternBuilder) =>
                        {
                            triplePatternBuilder
                                .Subject(x)
                                .PredicateUri(new Uri(@"https://schema.org/#recipeIngredient"))
                                .Object(y);
                        }).Filter((builder) => builder.Regex(builder.Variable("y"), input, "i"));

                //Console.WriteLine(queryBuilder.BuildQuery().ToString());
                //SparqlQuery query = queryBuilder.BuildQuery();
                var query = @"SELECT ?x WHERE { ?x <https://schema.org/#recipeIngredient> ?object
                    FILTER (REGEX(STR(?object), '{{input}}', 'i') )}";
                query = query.Replace("{{input}}", input);

                var results = graph.ExecuteQuery(query);

                //Print out the Results
                SparqlResultSet g = (SparqlResultSet)results;
                foreach (SparqlResult t in g)
                {
                    Console.WriteLine(t.ToString());
                }
                Console.ReadKey();
            }
            catch (RdfParseException parseEx)
            {
                //This indicates a parser error e.g unexpected character, premature end of input, invalid syntax etc.
                Console.WriteLine("Parser Error");
                Console.WriteLine(parseEx.Message);
            }
            catch (RdfException rdfEx)
            {
                //This represents a RDF error e.g. illegal triple for the given syntax, undefined namespace
                Console.WriteLine("RDF Error");
                Console.WriteLine(rdfEx.Message);
            }
        }
        String convertToSPARQLList(List<string> list)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            foreach (string item in list)
            {
                sb.Append("\"");
                sb.Append(item);
                sb.Append("\"");
                sb.Append(", ");
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(")");
            return sb.ToString();
        }
        static void ConvertJsonRecipesToJsonRdf()
        {
            object deserializedRecipes = JsonConvert.DeserializeObject<List<Recipe>>(File.ReadAllText(@"D:\Firefox Downloads\epicurious-recipes-with-rating-and-nutrition\full_format_recipes.json"));
            var recipes = deserializedRecipes as List<Recipe>;
            var template = File.ReadAllText(@"C:\Users\Danilo Popovikj\Desktop\Template.jsonld");
            var jsonArray = string.Empty;
            jsonArray += "[";
            foreach (var recipe in recipes)
            {
                var toReplace = new string(template.ToCharArray());
                toReplace = toReplace.Replace(@"{{Description}}", recipe.Desc?.Replace("\"", "") ?? string.Empty);
                var ingridientsString = string.Empty;

                if (recipe.Ingredients == null)
                    continue;

                foreach (var ing in recipe.Ingredients)
                {
                    ingridientsString += $"\"{ ing.Replace("\"", "")}\"";
                    ingridientsString += ",";
                }
                if (ingridientsString.EndsWith(','))
                    ingridientsString = ingridientsString.TrimEnd(ingridientsString[ingridientsString.Length - 1]);
                toReplace = toReplace.Replace("{{Ingredients}}", ingridientsString);
                toReplace = toReplace.Replace("{{Name}}", recipe.Title?.Replace("\"", "") ?? string.Empty);

                var instructionsString = string.Empty;
                foreach (var dir in recipe.Directions)
                {
                    instructionsString += $"\"{ dir.Replace("\"", "")}\"";
                    instructionsString += ",";
                }
                if (instructionsString.EndsWith(','))
                    instructionsString.TrimEnd(instructionsString[instructionsString.Length - 1]);
                toReplace = toReplace.Replace("{{Instructions}}", instructionsString);
                jsonArray += toReplace;
                jsonArray += ",";
            }
            if (jsonArray.EndsWith(','))
                jsonArray = jsonArray.TrimEnd(jsonArray[jsonArray.Length - 1]);
            jsonArray += ']';

            File.WriteAllText(@"C:\Users\Danilo Popovikj\Desktop\output.jsonld", jsonArray);
        }
    }
}
