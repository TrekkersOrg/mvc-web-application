/*const WebSocket = require('ws');
const { exec } = require('child_process');

const server = new WebSocket.Server({ port: 3000 });



server.on('connection', (socket) =>
{
    console.log('Client connected');
    socket.on('message',(message) =>
    {
        try
        {
            const data = JSON.parse(message);
            if (data["query"] && data["vectorstore"])
            {
                executeQuery(data.query,data.vectorstore,(response) =>
                {
                    console.log(response);
                    socket.send(JSON.stringify({ response }));
                });
            } else
            {
                socket.send(JSON.stringify({ error: 'Invalid message format' }));
            }
        } catch (error)
        {
            socket.send(JSON.stringify({ error: 'Invalid JSON format' }));
        }
    });

    socket.on('close',() =>
    {
        console.log('Client disconnected');
    })
})


function executeQuery(query,vectorstore,callback)
{
    const pythonProcess = exec(`py ../../Scripts/llm.py --query "${query}" --vectorstore "${vectorstore}"`,(error,stdout,stderr) =>
    {
        if (error)
        {
            console.error(`Error executing Python script: ${error.message}`);
            callback({ error: 'An error occurred while executing the Python script' });
            return;
        }
        const response = stdout.trim();
        console.log(`Python script output: ${response}`);
        callback({ response });
    });

    pythonProcess.stdin.end();
}*/