using DesafioFinal.BancoDeDados;
using DesafioFinal.BancoDeDados.DTOs;
using Microsoft.AspNetCore.StaticFiles;
using System.Collections;
using System.Drawing;
using System.Net;
using System.Runtime.Intrinsics.X86;

namespace DesafioFinal
{
    public static class EndpointPedidos
    {
        public static void MapPedidosEndpoint (this WebApplication app)
        {
            #region Resumo

            //Devolver o valor total(soma) dos pedidos de acordo com o mês e ano que foi realizado
            //Devolver o top 10 dos clientes que realizaram pedidos ordenados por ordem descrescente de valor
            //Para cada um dos meses que houve compra, devolver a soma dos valores comprados nos primeiros 15 dias do mês
            //e outra soma com os valores comprados nos últimos 15 dias do mês


            app.MapPost("/pedidos/resumo", async (InMemoryContext context) =>
            {
                
            });
            #endregion

            #region Mais comprados

            //Devolver os 30 produtos mais comprados(nome, categoria, quantidade e valor total), ordenados de forma decrescente pelo valor
            //Devolver os 30 produtos mais comprados, ordenados de forma decrescente pela quantidade

            app.MapPost("/pedidos/mais_comprados", async (InMemoryContext context) =>
            {
                #region Consulta produtos mais comprados por valor
                var produtosMaisCompradosPorValor = (from item in context.ItensDePedidos
                                       join produto in context.Produtos on item.product_id equals produto.product_id
                                       join categoria in context.Categorias on produto.category_id equals categoria.category_id
                                       group (new {item, produto, categoria }) by produto.product_id into produtosAgrupados
                                       orderby produtosAgrupados.Sum(prod => prod.item.quantity * prod.produto.price) descending
                                       select new
                                       {
                                           nome = produtosAgrupados.First().produto.product_name,
                                           categoria = produtosAgrupados.First().categoria.category_name,
                                           quantidade = produtosAgrupados.Sum(p => p.item.quantity),
                                           valor = produtosAgrupados.Sum(p => p.item.quantity * p.produto.price)
                                       }).Take(30);
                #endregion

                #region Consulta produtos mais comprados por quantidade
                var produtosMaisCompradosPorQuantidade = (from item in context.ItensDePedidos
                                                     join produto in context.Produtos on item.product_id equals produto.product_id
                                                     join categoria in context.Categorias on produto.category_id equals categoria.category_id
                                                     group (new { item, produto, categoria }) by produto.product_id into produtosAgrupados
                                                     orderby produtosAgrupados.Sum(prod => prod.item.quantity) descending
                                                     select new
                                                     {
                                                         nome = produtosAgrupados.First().produto.product_name,
                                                         categoria = produtosAgrupados.First().categoria.category_name,
                                                         quantidade = produtosAgrupados.Sum(p => p.item.quantity),
                                                         valor = produtosAgrupados.Sum(p => p.item.quantity * p.produto.price)
                                                     }).Take(30);
                #endregion

                return new { 
                    produtosMaisCompradosPorValor,
                    produtosMaisCompradosPorQuantidade

                };
            });
            #endregion

            #region Mais comprados por categoria

            //Devolver os 30 produtos mais comprados de cada uma das categorias(nome, quantidade e valor total),
            //ordenados de forma decrescente pelo valor
            //
            //Devolver os 30 produtos mais comprados de cada uma das categorias(nome, quantidade e valor total),
            //ordenados de forma decrescente pela quantidade
            //
            //Obs: O nome da categoria na chave NÃO deve conter acentos, caracteres especiais ou espaços.Ex: "Men's Shoes"
            //deve ser enviado como "mensShoes, "Play Sets Playground Equipment" deve ser enviado como "playSetsPlaygroundEquipment", etc.

            app.MapPost("/pedidos/mais_comprados_por_categoria", async (InMemoryContext context) =>
            {
                var produtosMaisCompradosPorValor = (from cat in context.Categorias
                                                     join prod in context.Produtos on cat.category_id equals prod.category_id
                                                     join item in context.ItensDePedidos on prod.product_id equals item.product_id
                                                     group new { cat, prod, item } by new {cat.category_name} into agrupadosPorCategoria
                                                     orderby agrupadosPorCategoria.Sum(prod => prod.item.quantity * prod.prod.price) descending
                                                     select agrupadosPorCategoria).Take(30);
                

                var produtosMaisCompradosPorQuantidade = (from cat in context.Categorias
                                                          join prod in context.Produtos on cat.category_id equals prod.category_id
                                                          join item in context.ItensDePedidos on prod.product_id equals item.product_id
                                                          group new { cat, prod, item } by new { prod.category_id, item.product_id } into agrupadosPorCategoria
                                                          orderby agrupadosPorCategoria.Sum(prod => prod.item.quantity) descending
                                                          select new
                                                          { 
                                                              nome = agrupadosPorCategoria.First().prod.product_name,
                                                              quantidade = agrupadosPorCategoria.Sum(p => p.item.quantity),
                                                              valor = agrupadosPorCategoria.Sum(p => p.item.quantity * p.prod.price).ToString()

                                                          }).Take(30);

                return new { produtosMaisCompradosPorValor, produtosMaisCompradosPorQuantidade };

            });
            #endregion

            #region Mais comprados por fornecedor

            //Devolver os 30 produtos mais comprados de cada um dos fornecedores(nome, categoria, quantidade e valor total),
            //ordenados de forma crescente pelo nome do fornecedor

            app.MapPost("/pedidos/mais_comprados_por_fornecedor", async (InMemoryContext context) =>
            {
                //var pedidosPorFornecedor = (from pedido in context.Pedidos
                //                           join item in context.ItensDePedidos on pedido.order_id equals item.order_id
                //                           join produto in context.Produtos on item.product_id equals produto.product_id
                //                           join fornec in context.Fornecedores on produto.supplier_id equals fornec.supplier_id
                //                           group new { pedido, item, produto, fornec } by fornec.supplier_id into agrupadoPorFornecedor
                //                           select agrupadoPorFornecedor).Take(5);

                var results = (from item in context.ItensDePedidos
                               join produto in context.Produtos on item.product_id equals produto.product_id
                               join fornecedor in context.Fornecedores on produto.supplier_id equals fornecedor.supplier_id
                               group (new { item, produto, fornecedor }) by fornecedor.supplier_name into groupedSuppliers
                               orderby groupedSuppliers.Key
                               select new
                               {
                                   Produtos = groupedSuppliers.Select(g => new
                                   {
                                       Nome = g.produto.product_name,
                                       Categoria = g.produto.category_id,
                                       Quantidade = g.item.quantity,
                                       Valor = g.produto.price
                                   })
                               }).ToList();
                return results;
            });
            #endregion
        }

    }
}
