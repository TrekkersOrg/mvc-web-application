import os
import sys
from re import S
from langchain.text_splitter import RecursiveCharacterTextSplitter
from langchain_community.embeddings import SentenceTransformerEmbeddings, HuggingFaceBgeEmbeddings
import pinecone
from langchain_community.vectorstores import Pinecone
import json
from PyPDF2 import PdfReader
import numpy
import argparse

class Document:
    def __init__(self, page_content, metadata=None):
        self.page_content = page_content
        self.metadata = metadata if metadata is not None else {}


def split_docs(documents,chunk_size=500,chunk_overlap=20):
  text_splitter = RecursiveCharacterTextSplitter(chunk_size=chunk_size, chunk_overlap=chunk_overlap)
  docs = text_splitter.split_documents(documents)
  return docs

def main():
    # Load shell configuration
    parser = argparse.ArgumentParser(description="Embed and index document into defined namespace.")
    parser.add_argument('-n', '--namespace', help='Specify the Pinecone namespace.', required=True)
    parser.add_argument('-d', '--debug', help='Provides additional information in output.', action='store_true')
    args = parser.parse_args()

    # Load configuration
    print("\n(1/6) Loading configuration...")
    with open('../appSettings.Development.json') as parser:
      appSettings = json.load(parser)
    if args.debug:
        print(appSettings)

    # Load documents
    print("\n(2/6) Loading document(s)...")
    reader = PdfReader('../UserFiles/Sample.Marital.Settlement.Agreement.10.15.2012.pdf')
    page = reader.pages[0] 
    text = page.extract_text() 
    documents = []
    documents.append(Document(page_content=text))
    if args.debug:
        print(documents)

    # Split document(s)
    print("\n(3/6) Splitting document(s)...")
    docs = split_docs(documents)
    if args.debug:
        print(docs)

    # Initialize embedding model
    print("\n(4/6) Initializing embedding model...")
    model_name = "all-MiniLM-L6-v2"
    embeddings = HuggingFaceBgeEmbeddings(model_name=model_name)
    if args.debug:
        print("Model Name: " + model_name)
        print(embeddings)

    # Initialize Pinecone index
    print("\n(5/6) Initializing Pinecone index...")
    pinecone.init(
        api_key = appSettings.get("Pinecone", {}).get("APIKey", None),
        environment= appSettings.get("Pinecone", {}).get("Environment", None)
    )
    index_name = appSettings.get("Pinecone", {}).get("Index", None)
    namespace = args.namespace
    try:
        index = Pinecone.from_documents(docs, embeddings, index_name=index_name, namespace=namespace)
    except Exception as e:
        print(e)
    if args.debug:
        print("Index Name: " + index_name)
        print("Namespace: " + namespace)
        print("Number of documents: " + str(len(docs)))

    print("\n(6/6) Finished.")


if __name__ == "__main__":
    main()
