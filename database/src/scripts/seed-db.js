require('dotenv').config();

const { MongoClient } = require('mongodb');

function getMongoUri() {
  return process.env.MONGODB_URI || 'mongodb://127.0.0.1:27017/device_management';
}

function getDbNameFromUri(uri) {
  const url = new URL(uri);
  const pathname = url.pathname || '';
  const dbFromPath = pathname.replace(/^\//, '');
  if (!dbFromPath) {
    throw new Error('MONGODB_URI must include a database URI');
  }
  return dbFromPath;
}

async function upsertByFilter(col, filter, doc) {
  const now = new Date();
  const update = {
    $set: { ...doc, updatedAt: now },
    $setOnInsert: { createdAt: now }
  };

  const res = await col.findOneAndUpdate(filter, update, { upsert: true, returnDocument: 'after' });

  if (res?.value) return res.value;

  const upsertedId = res?.lastErrorObject?.upserted;
  if (upsertedId) {
    return await col.findOne({ _id: upsertedId });
  }

  return await col.findOne(filter);
}

async function upsertUserByMatch(usersCol, { match, set }) {
  const matchQueries = [];

  if (match?.name && match?.location) matchQueries.push({ name: match.name, location: match.location });
  if (match?.name) matchQueries.push({ name: match.name });

  let existing = null;
  for (const q of matchQueries) {
    existing = await usersCol.findOne(q, { projection: { _id: 1 } });
    console.log(existing)
    if (existing?._id) break;
  }

  if (existing?._id) {
    const updated = await upsertByFilter(usersCol, { _id: existing._id }, set);
    return updated;
  }

  return await upsertByFilter(usersCol, { name: set.name, location: set.location }, set);
}

async function run() {
  const uri = getMongoUri();
  const dbName = getDbNameFromUri(uri);

  const client = new MongoClient(uri);
  await client.connect();


  try {
    const db = client.db(dbName);
    const users = db.collection('users');
    const devices = db.collection('devices');

    // Ensure indexes exist (safe to re-run; matches create-db.js).
    await Promise.all([
      users.createIndex({ name: 1, location: 1 }, { name: 'users_name_location' }),

      devices.createIndex({ name: 1, manufacturer: 1 }, { unique: true, name: 'devices_name_manufacturer_unique' }),
      devices.createIndex({ assignedToUserId: 1 }, { name: 'devices_assignedToUserId' }),
      devices.createIndex({ manufacturer: 1, type: 1, os: 1 }, { name: 'devices_make_type_os' })
    ]);

    const seededUsers = await Promise.all([
      upsertUserByMatch(users, {
        match: { name: 'Alex Johnson', location: 'London' },
        set: { name: 'Alex Johnson', role: 'IT Admin', location: 'London' }
      }),
      upsertUserByMatch(users, {
        match: { name: 'Priya Singh', location: 'Bucharest' },
        set: { name: 'Priya Singh', role: 'Finance', location: 'Bucharest' }
      }),
      upsertUserByMatch(users, {
        match: { name: 'Marco Rossi', location: 'Milan' },
        set: { name: 'Marco Rossi', role: 'Sales', location: 'Milan' }
      }),
      upsertUserByMatch(users, {
        match: { name: 'Nina Müller', location: 'Berlin' },
        set: { name: 'Nina Müller', role: 'HR', location: 'Berlin' }
      })
    ]);

    const userByKey = new Map(
      seededUsers
        .filter(Boolean)
        .map((u) => [`${u.name}__${u.location}`, u])
    );

    function requireUserId(name, location) {
      const u = userByKey.get(`${name}__${location}`);
      if (!u?._id) {
        throw new Error(`Seed failed: missing required user (name=${name}, location=${location})`);
      }
      return u._id;
    }

    const devicesToSeed = [
      {
        name: 'iPhone 15 Pro',
        manufacturer: 'Apple',
        type: 'phone',
        os: 'iOS',
        osVersion: '18.1',
        processor: 'A17 Pro',
        ramGb: 8,
        description: 'A high-performance Apple smartphone suitable for daily business use.',
        location: 'London',
        assignedToUserId: requireUserId('Alex Johnson', 'London'),
        assignedAt: new Date(Date.now() - 12 * 24 * 60 * 60 * 1000)
      },
      {
        name: 'Galaxy Tab S10',
        manufacturer: 'Samsung',
        type: 'tablet',
        os: 'Android',
        osVersion: '15',
        processor: 'Snapdragon 8 Gen 4',
        ramGb: 12,
        description: 'A large-screen Android tablet ideal for presentations and travel.',
        location: 'Milan',
        assignedToUserId: requireUserId('Marco Rossi', 'Milan'),
        assignedAt: new Date(Date.now() - 3 * 24 * 60 * 60 * 1000)
      },
      {
        name: 'Pixel 9',
        manufacturer: 'Google',
        type: 'phone',
        os: 'Android',
        osVersion: '15',
        processor: 'Tensor G5',
        ramGb: 12,
        description: 'A modern Android smartphone with a clean OS experience and strong security features.',
        location: 'Berlin',
        assignedToUserId: requireUserId('Nina Müller', 'Berlin'),
        assignedAt: new Date(Date.now() - 25 * 24 * 60 * 60 * 1000)
      },
      {
        name: 'iPad Air',
        manufacturer: 'Apple',
        type: 'tablet',
        os: 'iPadOS',
        osVersion: '18.0',
        processor: 'M2',
        ramGb: 8,
        description: 'A lightweight Apple tablet suitable for note-taking and productivity.',
        location: 'Bucharest',
        assignedToUserId: null,
        assignedAt: null
      }
    ];

    await Promise.all(
      devicesToSeed.map((d) =>
        upsertByFilter(devices, { name: d.name, manufacturer: d.manufacturer }, d)
      )
    );
  } finally {
    await client.close();
  }
}

run().catch((err) => {
  // eslint-disable-next-line no-console
  console.error(err);
  process.exitCode = 1;
});

