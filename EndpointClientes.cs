using DesafioFinal.BancoDeDados;
using DesafioFinal.BancoDeDados.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;

namespace DesafioFinal
{
    public static class EndpointClientes
    {
        public static void MapClientesEndpoint(this WebApplication app)
        {
            app.MapPost("/clientes", async (InMemoryContext context) => {
                IQueryable clientes = context.Clientes
                                    .OrderBy(c => c.first_name)
                                    .Select(c => new
                                    {
                                        nome_completo = $"{c.first_name} {c.last_name}",
                                        c.email,
                                        c.address,
                                        c.state,
                                        c.city,
                                        c.country
                                    })
                                    .Take(100);

                foreach (var cliente in clientes)
                {
                    await Console.Out.WriteLineAsync(cliente.ToString());
                }


                return new
                {
                    clientes
                };
            });


            //            Contador dos países com mais clientes. Caso não tenha país definido no cliente, devolver como "desconhecido"
            //            Top 5 domínios mais utilizados por clientes, com a quantidade de emails para o domínio

            app.MapPost("/clientes/resumo", async (InMemoryContext context) =>
            {
                #region Listagem de domínios mais frequentes
                List<string> dominiosFormatados = new List<string>();

                var consulta = context.Clientes.Select(c => c.email).ToList();
                foreach(string email in consulta)
                {
                    dominiosFormatados.Add(email.Split('@')[1]);
                    await Console.Out.WriteLineAsync(email.Split('@')[1]);
                }

                var dominiosFrequentes = dominiosFormatados
                                            .GroupBy(d => d)
                                            .OrderByDescending(d => d.Count())
                                            .Take(5);

                Dictionary<string, int> dominios = new Dictionary<string, int>();

                foreach (var dominio in dominiosFrequentes)
                {
                    await Console.Out.WriteLineAsync($"Domínio {dominio.Key}  -  Ocorrências: {dominio.Count()}");
                    dominios.Add(dominio.Key, dominio.Count());
                }

                #endregion

                #region Listagem de países mais frequentes
                var consultaPaises = context.Clientes
                                        .GroupBy(c => c.country)
                                        .Select(c => new
                                        {
                                            pais = c.Key,
                                            clientes = c.Count()
                                        })
                                        //.OrderByDescending(c => c)
                                        .Take(5)
                                        .AsEnumerable();

                Dictionary<string, int> paisesComMaisClientes = new Dictionary<string, int>();

                foreach(var item in consultaPaises)
                {
                    await Console.Out.WriteLineAsync($"{item.pais} - {item.clientes}");
                    if(item.pais == "-")
                    {
                        paisesComMaisClientes.Add("desconhecido", item.clientes);
                    }
                    else
                    {
                        paisesComMaisClientes.Add(item.pais, item.clientes);
                    }
                }


                #endregion

                return new
                {
                    paisesComMaisClientes,
                    dominios
                };
            });
        }

    }
}
