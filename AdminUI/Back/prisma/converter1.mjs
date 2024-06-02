import fs from 'fs';
import camelCase from 'camelcase';

const schemaFilePath = 'prisma/schema.prisma';

// Read the schema file
fs.readFile(schemaFilePath, 'utf8', (err, data) => {
  if (err) {
    console.error('Error reading schema file:', err);
    return;
  }

  // Process the schema
  const processedSchema = processSchema(data);

  // Write the modified schema back to the file
  fs.writeFile(schemaFilePath, processedSchema, 'utf8', (err) => {
    if (err) {
      console.error('Error writing schema file:', err);
    } else {
      console.log('Schema file updated successfully.');
    }
  });
});

function processSchema(schema) {
  const lines = schema.split('\n');
  const processedLines = lines.map((line) => {
    if (line.trim().startsWith('@@') || line.trim().startsWith('//')) {
      return line; // Skip schema-level attributes and comments
    }

    const match = line.match(/\s+(\w+)\s+(\w+)(\??)\s+(@.*)?/);
    if (match) {
      const [fullMatch, name, type, optional, attributes] = match;
      const camelCasedName = camelCase(name);
      const mapAttribute = `@map("${name}")`;

      // Check if @map attribute already exists
      const newAttributes = attributes
        ? attributes.includes('@map')
          ? attributes
          : `${attributes} ${mapAttribute}`
        : mapAttribute;

      return line.replace(fullMatch, `  ${camelCasedName} ${type}${optional} ${newAttributes}`);
    }

    return line;
  });

  return processedLines.join('\n');
}