using FI.AtividadeEntrevista.BLL;
using WebAtividadeEntrevista.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FI.AtividadeEntrevista.DML;

namespace WebAtividadeEntrevista.Controllers
{
    public class ClienteController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Incluir()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Incluir(ClienteModel model)
        {
            BoCliente bo = new BoCliente();

            if (!this.ModelState.IsValid)
            {
                List<string> erros = (from item in ModelState.Values
                                      from error in item.Errors
                                      select error.ErrorMessage).ToList();

                Response.StatusCode = 400;
                return Json(string.Join(Environment.NewLine, erros));
            }
            else if (bo.ValidaCPF(model.CPF))
            {
                if (!bo.VerificarExistencia(model.CPF))
                {
                    bool liberate = true;

                    foreach (var item in model.Beneficiarios)
                    {
                        if (!bo.ValidaCPF(item.CPF))
                        {
                            liberate = false;
                            break;
                        }
                    }

                    if (liberate)
                    {
                        List<string> listCpfRegisters = new List<string>();

                        foreach (var item in model.Beneficiarios)
                        {
                            if (bo.VerificarExistenciaBeneficiario(item.CPF))
                            {
                                listCpfRegisters.Add(item.CPF);
                            }
                        }

                        if (listCpfRegisters.Count > 0)
                        {
                            string erro = "O(s) seguinte(s) CPF(s) já esta(ão) cadastrado(s): ";

                            foreach (var item in listCpfRegisters)
                            {
                                erro += item + ",";
                            }

                            erro = erro.Remove(erro.Length - 1);

                            Response.StatusCode = 400;
                            return Json(string.Join(Environment.NewLine, erro));
                        }
                        else
                        {
                            model.Id = bo.Incluir(new Cliente()
                            {
                                CEP = model.CEP,
                                Cidade = model.Cidade,
                                Email = model.Email,
                                Estado = model.Estado,
                                Logradouro = model.Logradouro,
                                Nacionalidade = model.Nacionalidade,
                                CPF = model.CPF,
                                Nome = model.Nome,
                                Sobrenome = model.Sobrenome,
                                Telefone = model.Telefone
                            });

                            foreach (var item in model.Beneficiarios)
                            {
                                item.IdCliente = model.Id;

                                bo.IncluirBeneficiario(new Beneficiarios()
                                {
                                    CPF = item.CPF,
                                    Nome = item.Nome,
                                    IdCliente = item.IdCliente
                                });

                            }

                            return Json("Cadastro efetuado com sucesso");
                        }
                    }
                    else
                    {
                        string erro = "CPF do Benificiario é invalido";

                        Response.StatusCode = 400;
                        return Json(string.Join(Environment.NewLine, erro));
                    }
                }
                else
                {
                    string erro = "Este CPF já encontra-se cadastrado!";

                    Response.StatusCode = 400;
                    return Json(string.Join(Environment.NewLine, erro));
                }
            }
            else
            {
                string erro = "CPF digitado não é válido!";

                Response.StatusCode = 400;
                return Json(string.Join(Environment.NewLine, erro));
            }
        }

        [HttpPost]
        public JsonResult Alterar(ClienteModel model)
        {
            BoCliente bo = new BoCliente();

            if (!this.ModelState.IsValid)
            {
                List<string> erros = (from item in ModelState.Values
                                      from error in item.Errors
                                      select error.ErrorMessage).ToList();

                Response.StatusCode = 400;
                return Json(string.Join(Environment.NewLine, erros));
            }
            else if (bo.ValidaCPF(model.CPF))
            {
                bo.Alterar(new Cliente()
                {
                    CEP = model.CEP,
                    Cidade = model.Cidade,
                    Email = model.Email,
                    Estado = model.Estado,
                    Logradouro = model.Logradouro,
                    Nacionalidade = model.Nacionalidade,
                    CPF = model.CPF,
                    Nome = model.Nome,
                    Sobrenome = model.Sobrenome,
                    Telefone = model.Telefone
                });

                return Json("Cadastro efetuado com sucesso");
            }
            else
            {
                string erro = "CPF digitado não é válido!";

                Response.StatusCode = 400;
                return Json(string.Join(Environment.NewLine, erro));
            }
        }

        [HttpGet]
        public ActionResult Alterar(long id)
        {
            BoCliente bo = new BoCliente();
            Cliente cliente = bo.Consultar(id);
            Models.ClienteModel model = null;

            if (cliente != null)
            {
                model = new ClienteModel()
                {
                    Id = cliente.Id,
                    CEP = cliente.CEP,
                    Cidade = cliente.Cidade,
                    Email = cliente.Email,
                    Estado = cliente.Estado,
                    Logradouro = cliente.Logradouro,
                    Nacionalidade = cliente.Nacionalidade,
                    CPF = cliente.CPF,
                    Nome = cliente.Nome,
                    Sobrenome = cliente.Sobrenome,
                    Telefone = cliente.Telefone,
                    Beneficiarios = new List<Beneficiarios>(),
                };

                model.Beneficiarios = bo.ConsultarBeneficiarios(model.Id);

            }

            return View(model);
        }

        [HttpPost]
        public JsonResult ClienteList(int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {
            try
            {
                int qtd = 0;
                string campo = string.Empty;
                string crescente = string.Empty;
                string[] array = jtSorting.Split(' ');

                if (array.Length > 0)
                    campo = array[0];

                if (array.Length > 1)
                    crescente = array[1];

                List<Cliente> clientes = new BoCliente().Pesquisa(jtStartIndex, jtPageSize, campo, crescente.Equals("ASC", StringComparison.InvariantCultureIgnoreCase), out qtd);

                //Return result to jTable
                return Json(new { Result = "OK", Records = clientes, TotalRecordCount = qtd });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

    }
}