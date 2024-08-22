import puppeteer from "puppeteer";

function makeid(length: number) {
  let result = "";
  const characters = "abcdefghijklmnopqrstuvwxyz";
  const charactersLength = characters.length;
  let counter = 0;
  while (counter < length) {
    result += characters.charAt(Math.floor(Math.random() * charactersLength));
    counter += 1;
  }
  return result;
}

const sleep = (value: number) => new Promise((r) => setTimeout(r, value));

async function run() {
  const browser = await puppeteer.launch({
    headless: true, // Set to false to see the browser in action
  });
  const page = await browser.newPage();
  page.setDefaultNavigationTimeout(5 * 60 * 1000);
  page.setDefaultTimeout(5 * 60 * 1000);
  await page.goto("https://poll.w.thera-engineering.com/game/7ebb170d-31e5-43e8-b663-5603230ebab1");

  await page.waitForSelector("#firstname");

  const username = "R-" + makeid(6);
  await page.type("#firstname", username);

  await page.click('[type="submit"]');

  while (true) {
    console.log("Start of loop. Pausing for a second.");
    await sleep(1000);

    try {
      console.log("Waiting for a question to be clickable.");
      await page.waitForSelector(".question", { timeout: 3_600_000 });
      const answers = await page.$$(".question");
      if (answers.length === 0) {
        console.log("Found no answer. Looping.");
        continue;
      }
      // select random answer
      const rnd = Math.floor(Math.random() * answers.length);
      const selected = answers[rnd];

      const sleepTime = Math.floor(Math.random() * 2000) + 1000;
      console.log("Sleeping for " + sleepTime + "ms");
      await sleep(sleepTime);
      await selected.click();

      console.log("Waiting for answer success or failure");
      const found = await page.waitForSelector(".answer-failure,.answer-success", { timeout: 0 });
      const propertyClass = await found?.getProperty("className");
      console.log("Element: ", await propertyClass?.jsonValue());
    } catch (e) {
      console.error(e);
      console.log('Continuing');
    }
  }

  await browser.close();
}

run().catch((error) => console.error("Error:", error));
