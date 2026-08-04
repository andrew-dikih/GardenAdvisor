import React from 'react';
import { render, screen } from '@testing-library/react';
import App from './App';

test('renders the address page as the default route', () => {
  render(<App />);
  // AddressPage is the "/" route; this button is always present on load,
  // regardless of search/map state, making it a stable smoke-test anchor.
  const continueButton = screen.getByText(/continue to plant selection/i);
  expect(continueButton).toBeInTheDocument();
});

