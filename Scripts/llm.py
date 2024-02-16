from langchain_community.vectorstores import Pinecone
import openai
from langchain.chat_models import ChatOpenAI
from langchain.chains.conversation.memory import ConversationBufferWindowMemory
from langchain.chains import RetrievalQA
from langchain_community.embeddings import SentenceTransformerEmbeddings, HuggingFaceBgeEmbeddings
import json
import pinecone
import argparse


def main():
    # Load shell configuration
    parser = argparse.ArgumentParser(description="Run client query using target Pinecone vector store.")
    parser.add_argument('-q', '--query', help='Enter your query.', required=True)
    parser.add_argument('-v', '--vectorstore', help='Namespace in Pinecone index.', required=True)
    args = parser.parse_args()

    # Load configuration
    print("\n(1/6) Loading configuration...")
    with open('../appSettings.Development.json') as parser:
        appSettings = json.load(parser)

    # Initialize embedding model
    print("\n(2/6) Initializing Huggingface embedding model...")
    model_name = "all-MiniLM-L6-v2"
    embeddings = HuggingFaceBgeEmbeddings(model_name=model_name)

    # Load vector store
    print("\n(3/6) Loading vector store...")
    pinecone.init(
        api_key = appSettings.get("Pinecone", {}).get("APIKey", None),
        environment= appSettings.get("Pinecone", {}).get("Environment", None)
    )
    vectorstore = Pinecone.from_existing_index(index_name="langchain-chatbot", embedding=embeddings, namespace=args.vectorstore)

    # Initialize LLM
    print("\n(4/6) Initializing OpenAI GPT 3.5 Turbo LLM model...")
    llm = ChatOpenAI(openai_api_key = appSettings.get("OpenAI", {}).get("SecretKey", None),
        model_name = appSettings.get("OpenAI", {}).get("Model", None),
        temperature = 0.0)

    # Initilize retrieval QA
    print("\n(5/6) Initializing conversation memory buffer and LangChain retrieval QA...")
    conv_mem = ConversationBufferWindowMemory(
        memory_key = 'history',
        k = 5,
        return_messages =True)

    qa = RetrievalQA.from_chain_type(
        llm = llm,
        chain_type = "stuff",
        retriever = vectorstore.as_retriever())


    # Run Query
    print("\n(6/6) Loading LLM response to query...")
    response = qa.run(args.query)
    print(response)
    return response

if __name__ == "__main__":
    main()
