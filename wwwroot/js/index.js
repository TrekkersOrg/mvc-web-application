document.addEventListener('DOMContentLoaded',() =>
{
    // Initialize the spider chart
    const initSpiderChart = () =>
    {
        const ctx = document.getElementById('spiderChart').getContext('2d');
        return new Chart(ctx,{
            type: 'radar',
            data: {
                labels: ['Operational Risk','','Compliance Risk',''],
                datasets: [{
                    data: [3,2,4,5],
                    backgroundColor: 'rgba(255, 191, 0, 0.1)',
                    borderColor: 'rgba(255, 191, 0, 1)',
                    borderWidth: 1
                }]
            },
            options: {
                scales: {
                    r: {
                        angleLines: { color: '#ffffff' },
                        grid: { color: '#ffffff' },
                        pointLabels: {
                            color: '#ffffff',
                            font: { size: 14 }
                        },
                        ticks: {
                            beginAtZero: true,
                            max: 5,
                            display: false
                        }
                    }
                },
                plugins: {
                    legend: { display: false }
                },
                responsive: true,
                maintainAspectRatio: false,
            }
        });
    };

    // Smooth scrolling to section
    const setupSmoothScroll = () =>
    {
        document.querySelector('.btn.btn-primary-demo').addEventListener('click',e =>
        {
            e.preventDefault();
            document.querySelector('#middle-section').scrollIntoView({ behavior: 'smooth' });
        });
    };

    // Generate random data between 0 and 5
    const generateRandomData = () => Array.from({ length: 4 },() => Math.random() * 5);

    // Update the spider chart with new data
    const updateSpiderChartData = (chart,newData) =>
    {
        chart.data.datasets[0].data = newData;
        chart.update();
    };

    // Initialize the chart and start updating data every 2 seconds
    const startChartUpdater = (chart) =>
    {
        setInterval(() =>
        {
            const newData = generateRandomData();
            updateSpiderChartData(chart,newData);
        },2000);
    };

    // Main function to run the script
    const main = () =>
    {
        const spiderChart = initSpiderChart();
        setupSmoothScroll();
        startChartUpdater(spiderChart);
    };

    main();
});
