from langchain.text_splitter import RecursiveCharacterTextSplitter
from langchain.embeddings import SentenceTransformerEmbeddings, HuggingFaceBgeEmbeddings
import pinecone
from langchain.vectorstores import Pinecone
import json
from PyPDF2 import PdfReader
import numpy

class Document:
    def __init__(self, page_content, metadata=None):
        self.page_content = page_content
        self.metadata = metadata if metadata is not None else {}


def split_docs(documents,chunk_size=500,chunk_overlap=20):
  text_splitter = RecursiveCharacterTextSplitter(chunk_size=chunk_size, chunk_overlap=chunk_overlap)
  docs = text_splitter.split_documents(documents)
  return docs

# Load configuration
print("Loading configuration...")
<<<<<<< HEAD
with open('../appSettings.json') as parser:
  appSettings = json.load(parser)

=======
with open('../appSettings.Development.json') as parser:
  appSettings = json.load(parser)

print(appSettings)
>>>>>>> 4f0e8c72428d90316cd7d07b5e401fe3a5a4d98d
print("Loading document(s)...")
reader = PdfReader('../UserFiles/Sample.Marital.Settlement.Agreement.10.15.2012.pdf')
page = reader.pages[0] 
text = page.extract_text() 
documents = []
documents.append(Document(page_content=text))

# Split document(s)
print("Splitting document(s)...")
docs = split_docs(documents)

# Initialize embedding model
print("Initializing embedding model...")
embeddings = HuggingFaceBgeEmbeddings(model_name="all-MiniLM-L6-v2")

# Initialize Pinecone index
print("Initializing Pinecone index...")
pinecone.init(
    api_key = appSettings.get("Pinecone", {}).get("APIKey", None),
    environment= appSettings.get("Pinecone", {}).get("Environment", None)
)
index_name = appSettings.get("Pinecone", {}).get("Index", None)
index = Pinecone.from_documents(docs, embeddings, index_name=index_name)

