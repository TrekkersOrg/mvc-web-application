import pandas as pd
import os
from tqdm.auto import tqdm
import time
import pinecone
import openai
from langchain.embeddings.openai import OpenAIEmbeddings
from langchain.vectorstores import Pinecone
from langchain.chat_models import ChatOpenAI
from langchain.chains.conversation.memory import ConversationBufferWindowMemory
from langchain.chains import RetrievalQA
import json

with open('../appSettings.Development.json') as parser:
      appSettings = json.load(parser)


def initialize_vectorsore(namespace):
    return ""

print(initialize_vectorsore("Source"))
