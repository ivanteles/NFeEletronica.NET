﻿#region

using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;

#endregion

namespace NFeEletronica.Assinatura
{
    public class Xml
    {
        private string _validarResultado = "";

        /// <summary>
        ///     Valida se um Xml está seguindo de acordo um Schema
        /// </summary>
        /// <param name="arquivoXml">Arquivo Xml</param>
        /// <param name="arquivoSchema">Arquivo de Schema</param>
        /// <returns>True se estiver certo, Erro se estiver errado</returns>
        public void ValidaSchema(string arquivoXml, string arquivoSchema)
        {
            //Seleciona o arquivo de schema de acordo com o schema informado
            //arquivoSchema = Bll.Util.ContentFolderSchemaValidacao + "\\" + arquivoSchema;

            //Verifica se o arquivo de XML foi encontrado.
            if (!File.Exists(arquivoXml))
                throw new Exception("Arquivo de XML informado: \"" + arquivoXml + "\" não encontrado.");

            //Verifica se o arquivo de schema foi encontrado.
            if (!File.Exists(arquivoSchema))
                throw new Exception("Arquivo de schema: \"" + arquivoSchema + "\" não encontrado.");

            // Cria um novo XMLValidatingReader
            var reader = new XmlValidatingReader(new XmlTextReader(new StreamReader(arquivoXml)));
            // Cria um schemacollection
            var schemaCollection = new XmlSchemaCollection();
            //Adiciona o XSD e o namespace
            schemaCollection.Add("http://www.portalfiscal.inf.br/nfe", arquivoSchema);
            // Adiciona o schema ao ValidatingReader
            reader.Schemas.Add(schemaCollection);
            //Evento que retorna a mensagem de validacao
            reader.ValidationEventHandler += Reader_ValidationEventHandler;
            //Percorre o XML
            while (reader.Read())
            {
            }
            reader.Close(); //Fecha o arquivo.
            //O Resultado é preenchido no reader_ValidationEventHandler

            if (_validarResultado != "")
                throw new Exception(_validarResultado);
        }

        /// <summary>
        ///     Se der um erro na validação do schema, esse evento é disparado
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Reader_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            // Como sera exibida a mensagem de ERROS de validacao
            _validarResultado = _validarResultado + string.Format("\rLinha:{1}" + Environment.NewLine +
                                                                "\rColuna:{0}" + Environment.NewLine +
                                                                "\rErro:{2}" + Environment.NewLine,
                                   e.Exception.LinePosition,
                                   e.Exception.LineNumber,
                                   e.Exception.Message);
        }

        public static XmlDocument StringToXml(string stringText)
        {
            var xml = new XmlDocument();
            xml.LoadXml(stringText);

            return xml;
        }
    }
}